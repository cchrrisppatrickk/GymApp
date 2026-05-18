using GymApp.Models;
using GymApp.Repositories;
using GymApp.ViewModels;
using GymApp.ViewModels.ApiAgent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GymApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services
{
    public class PagoService : IPagoService
    {
        private readonly IPagoRepository _pagoRepo;
        private readonly IMembresiaRepository _membresiaRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly GymDbContext _context;
        private readonly IWebhookService _webhookService;
        private readonly IConfiguracionAlertaRepository _configRepo;
        private readonly IWebHostEnvironment _env;

        public PagoService(
            IPagoRepository pagoRepo,
            IMembresiaRepository membresiaRepo,
            IUsuarioRepository usuarioRepo,
            GymDbContext context,
            IWebhookService webhookService,
            IConfiguracionAlertaRepository configRepo,
            IWebHostEnvironment env)
        {
            _pagoRepo = pagoRepo;
            _membresiaRepo = membresiaRepo;
            _usuarioRepo = usuarioRepo;
            _context = context;
            _webhookService = webhookService;
            _configRepo = configRepo;
            _env = env;
        }

        public async Task<List<DeudaInfoDTO>> BuscarDeudaClienteAsync(string termino)
        {
            var todas = await _membresiaRepo.ObtenerTodasConDetallesAsync();
            var membresias = todas.Where(m => (m.Estado == "Activa" || m.Estado == "Pendiente Pago" || m.Estado == "Vencida") && 
                (string.IsNullOrEmpty(termino) || 
                 m.User.NombreCompleto.Contains(termino, StringComparison.OrdinalIgnoreCase) || 
                 (m.User.Dni != null && m.User.Dni.Contains(termino)))).ToList();

            var resultados = new List<DeudaInfoDTO>();

            foreach(var membresia in membresias)
            {
                if (membresia.Plan == null) continue;

                decimal precioAcordado = membresia.PrecioAcordado;
                // Usamos la relación ya cargada para evitar N+1 queries
                decimal yaPagado = membresia.PagosMembresia != null 
                    ? membresia.PagosMembresia.Where(p => !p.EsAnulado).Sum(p => p.Monto)
                    : 0;
                
                decimal deuda = precioAcordado - yaPagado;

                // Solo mostramos membresías que tengan deuda pendiente (> 0)
                if (deuda > 0)
                {
                    resultados.Add(new DeudaInfoDTO
                    {
                        MembresiaId = membresia.MembresiaId,
                        NombreCliente = membresia.User.NombreCompleto,
                        DniCliente = membresia.User.Dni,
                        NombrePlan = membresia.Plan.Nombre,
                        Estado = membresia.FechaVencimiento < DateOnly.FromDateTime(DateTime.Now) && membresia.Estado != "Pendiente Pago" ? "Vencida" : membresia.Estado,
                        PrecioTotal = precioAcordado,
                        TotalPagado = yaPagado,
                        DeudaPendiente = deuda
                    });
                }
            }

            return resultados;
        }

        public async Task<int> RegistrarPagoAsync(PagoCreateDTO dto, int empleadoId)
        {
            // 1. Validaciones
            if (dto.Monto <= 0) throw new Exception("El monto debe ser mayor a 0");

            var membresia = await _membresiaRepo.ObtenerPorIdConDetallesAsync(dto.MembresiaId);
            if (membresia == null || membresia.Plan == null) throw new Exception("Membresía no encontrada o no tiene un plan válido asociado.");

            decimal precioAcordado = membresia.PrecioAcordado;
            decimal yaPagado = await _pagoRepo.GetTotalPagadoAsync(dto.MembresiaId);
            decimal deudaPendiente = precioAcordado - yaPagado;

            if (dto.Monto > deudaPendiente)
                throw new Exception($"El monto excede la deuda. Solo debe: {deudaPendiente:C}");

            // 2. Guardar comprobante (si aplica) y registrar el Pago
            string? rutaComprobante = await ProcesarComprobanteAsync(dto, empleadoId);

            var nuevoPago = new PagosMembresium
            {
                MembresiaId = dto.MembresiaId,
                UsuarioEmpleadoId = empleadoId,
                Monto = dto.Monto,
                MetodoPago = dto.MetodoPago,
                FechaPago = dto.FechaPago ?? DateTime.Now,
                Comprobante = rutaComprobante
            };

            await _pagoRepo.InsertAsync(nuevoPago);

            // 3. ACTUALIZACIÓN DE ESTADO (El paso crucial que faltaba)
            // Verificamos si con este pago se salda la deuda
            decimal nuevoTotalPagado = yaPagado + dto.Monto;

            if (nuevoTotalPagado >= precioAcordado)
            {
                if (membresia.Estado == "Pendiente Pago") // Solo si estaba pendiente
                {
                    membresia.Estado = "Activa";
                    await _membresiaRepo.UpdateAsync(membresia);
                }
            }

            // 4. Guardar todo (Pago + Actualización de Membresía si hubo) en una transacción
            await _pagoRepo.SaveAsync();

            // --- NOTIFICACIÓN EN TIEMPO REAL ---
            var configs = await _configRepo.GetAllAsync();
            var configsParaNotificar = configs.Where(c => c.Activo && c.AvisarNuevoPago).ToList();

            if (configsParaNotificar.Any())
            {
                var pagoDetalle = await _context.PagosMembresia
                    .Include(p => p.Membresia)
                    .ThenInclude(m => m.User)
                    .FirstOrDefaultAsync(p => p.PagoId == nuevoPago.PagoId);

                if (pagoDetalle != null)
                {
                    var payload = new 
                    { 
                        NombreCliente = pagoDetalle.Membresia.User.NombreCompleto, 
                        PagoID = pagoDetalle.PagoId, 
                        Monto = pagoDetalle.Monto, 
                        MetodoPago = pagoDetalle.MetodoPago, 
                        FechaHora = pagoDetalle.FechaPago, 
                        Observaciones = pagoDetalle.Observaciones ?? "Ninguna" 
                    };

                    foreach (var config in configsParaNotificar)
                    {
                        await _webhookService.EnviarAlertaInstantaneaAsync("NUEVO_PAGO", payload, config.ChatIdDestino);
                    }
                }
            }

            return nuevoPago.PagoId;
        }

        public async Task ActualizarPagoAsync(PagoEditDTO dto, int empleadoId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pago = await _context.PagosMembresia
                    .Include(p => p.Membresia)
                    .ThenInclude(m => m.PagosMembresia)
                    .FirstOrDefaultAsync(p => p.PagoId == dto.Id);

                if (pago == null)
                    throw new Exception("Pago no encontrado.");

                if (pago.EsAnulado)
                    throw new Exception("No se puede editar un pago que ha sido anulado.");

                decimal otrosPagos = pago.Membresia.PagosMembresia
                    .Where(p => p.PagoId != dto.Id && !p.EsAnulado)
                    .Sum(p => p.Monto);

                decimal maximoPermitido = pago.Membresia.PrecioAcordado - otrosPagos;

                if (dto.Monto > maximoPermitido)
                    throw new Exception($"El monto excede la deuda pendiente. El máximo permitido es: {maximoPermitido:C}");

                pago.Monto = dto.Monto;
                pago.MetodoPago = dto.MetodoPago;
                pago.Observaciones = $"Editado: {dto.Observaciones}".Trim();

                _context.PagosMembresia.Update(pago);
                await _context.SaveChangesAsync();

                await SincronizarEstadoMembresiaAsync(pago.MembresiaId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AnularPagoAsync(int id, string motivoAnulacion, int empleadoId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pago = await _context.PagosMembresia
                    .Include(p => p.Membresia)
                    .ThenInclude(m => m.PagosMembresia)
                    .FirstOrDefaultAsync(p => p.PagoId == id);

                if (pago == null)
                    throw new Exception("Pago no encontrado.");

                if (pago.EsAnulado)
                    throw new Exception("El pago ya se encuentra anulado.");

                pago.EsAnulado = true;
                pago.Observaciones = $"Anulado por emp {empleadoId}: {motivoAnulacion}";

                _context.PagosMembresia.Update(pago);
                await _context.SaveChangesAsync();

                await SincronizarEstadoMembresiaAsync(pago.MembresiaId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task SincronizarEstadoMembresiaAsync(int membresiaId)
        {
            var membresia = await _context.Membresias
                .Include(m => m.PagosMembresia)
                .FirstOrDefaultAsync(m => m.MembresiaId == membresiaId);

            if (membresia == null) return;

            decimal totalPagado = membresia.PagosMembresia
                .Where(p => !p.EsAnulado)
                .Sum(p => p.Monto);

            decimal deudaActual = membresia.PrecioAcordado - totalPagado;

            if (deudaActual > 0)
            {
                membresia.Estado = "Pendiente Pago";
            }
            else if (deudaActual <= 0 && membresia.Estado != "Vencida" && membresia.Estado != "Congelada")
            {
                membresia.Estado = "Activa";
            }

            _context.Membresias.Update(membresia);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PagoListDTO>> ListarPagosAsync()
        {
            var pagos = await _pagoRepo.ObtenerHistorialCompletoAsync();
            return pagos.Select(p => new PagoListDTO
            {
                PagoId = p.PagoId,
                NombreCliente = p.Membresia.User.NombreCompleto,
                NombrePlan = p.Membresia.Plan.Nombre,
                Monto = p.Monto,
                MetodoPago = p.MetodoPago,
                FechaPago = p.FechaPago?.ToString("g"), // Formato general fecha+hora
                NombreEmpleado = p.UsuarioEmpleado.NombreCompleto,
                EsAnulado = p.EsAnulado
            });
        }

        public async Task<PagedResult<PagoListDTO>> ObtenerPagosPaginadosAsync(string? buscar, int? mes, int? anio, int pagina, int tamanoPagina = 20)
        {
            var query = _context.PagosMembresia
                .Include(p => p.Membresia).ThenInclude(m => m.User)
                .Include(p => p.Membresia).ThenInclude(m => m.Plan)
                .Include(p => p.UsuarioEmpleado)
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
            {
                var termino = buscar.ToLower();
                query = query.Where(p => p.Membresia.User.NombreCompleto.ToLower().Contains(termino) || 
                                         (p.Membresia.User.Dni != null && p.Membresia.User.Dni.Contains(termino)));
            }
            else if (mes.HasValue && anio.HasValue)
            {
                query = query.Where(p => p.FechaPago.HasValue && p.FechaPago.Value.Month == mes.Value && p.FechaPago.Value.Year == anio.Value);
            }

            query = query.OrderByDescending(p => p.PagoId);

            int totalRegistros = await query.CountAsync();
            var items = await query.Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina).ToListAsync();

            var listaDto = items.Select(p => new PagoListDTO
            {
                PagoId = p.PagoId,
                NombreCliente = p.Membresia.User.NombreCompleto,
                NombrePlan = p.Membresia.Plan.Nombre,
                Monto = p.Monto,
                MetodoPago = p.MetodoPago,
                FechaPago = p.FechaPago?.ToString("dd/MM/yyyy HH:mm") ?? "--- ---",
                NombreEmpleado = p.UsuarioEmpleado.NombreCompleto,
                EsAnulado = p.EsAnulado
            }).ToList();

            return new PagedResult<PagoListDTO>
            {
                Items = listaDto,
                TotalPages = (int)Math.Ceiling((double)totalRegistros / tamanoPagina),
                CurrentPage = pagina,
                SearchTerm = buscar
            };
        }

        // --- Para ApiAgent ---
        public async Task<DeudaUsuarioAgenteDTO> ObtenerDeudaTotalParaAgenteAsync(int userId)
        {
            var membresias = await _context.Membresias
                .Include(m => m.PagosMembresia)
                .Where(m => m.UserId == userId)
                .ToListAsync();

            decimal deudaTotal = 0;
            int membresiasConDeuda = 0;

            foreach (var m in membresias)
            {
                decimal pagado = m.PagosMembresia.Sum(p => p.Monto);
                decimal deudaMembresia = m.PrecioAcordado - pagado;
                
                if (deudaMembresia > 0)
                {
                    deudaTotal += deudaMembresia;
                    membresiasConDeuda++;
                }
            }

            return new DeudaUsuarioAgenteDTO
            {
                DeudaTotal = deudaTotal,
                MembresiasConDeuda = membresiasConDeuda
            };
        }

        public async Task<IEnumerable<PagoAgenteDTO>> ObtenerHistorialUsuarioParaAgenteAsync(int userId)
        {
            var pagos = await _context.PagosMembresia
                .Include(p => p.Membresia)
                .ThenInclude(m => m.User)
                .Where(p => p.Membresia.UserId == userId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            return pagos.Select(p => new PagoAgenteDTO
            {
                Id = p.PagoId,
                Monto = p.Monto,
                Fecha = p.FechaPago ?? DateTime.MinValue,
                MetodoPago = p.MetodoPago ?? "Desconocido",
                NombreCliente = p.Membresia.User.NombreCompleto
            });
        }

        public async Task<IEnumerable<PagoAgenteDTO>> ObtenerPagosPorRangoParaAgenteAsync(DateTime inicio, DateTime fin)
        {
            var pagos = await _context.PagosMembresia
                .Include(p => p.Membresia)
                .ThenInclude(m => m.User)
                .Where(p => p.FechaPago >= inicio && p.FechaPago <= fin)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            return pagos.Select(p => new PagoAgenteDTO
            {
                Id = p.PagoId,
                Monto = p.Monto,
                Fecha = p.FechaPago ?? DateTime.MinValue,
                MetodoPago = p.MetodoPago ?? "Desconocido",
                NombreCliente = p.Membresia.User.NombreCompleto
            });
        }

        // ── CONSULTA DE DETALLE ─────────────────────────────────────────────
        public async Task<PagoDetalleDTO?> ObtenerDetallePagoAsync(int pagoId)
        {
            var pago = await _context.PagosMembresia
                .Include(p => p.Membresia)
                    .ThenInclude(m => m.User)
                .Include(p => p.Membresia)
                    .ThenInclude(m => m.Plan)
                .Include(p => p.Membresia)
                    .ThenInclude(m => m.PagosMembresia)   // todos los pagos de la membresía
                .Include(p => p.UsuarioEmpleado)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PagoId == pagoId);

            if (pago == null) return null;

            // ── Cálculo de deuda ──────────────────────────────────────────────
            // Tomamos solo los pagos NO anulados, ordenados cronológicamente.
            // El acumulado de ESTE pago = suma de todos los pagos válidos cuyo
            // (FechaPago, PagoId) es anterior o igual al pago consultado.
            var pagosValidos = pago.Membresia.PagosMembresia
                .Where(p2 => !p2.EsAnulado)
                .OrderBy(p2 => p2.FechaPago)
                .ThenBy(p2 => p2.PagoId)
                .ToList();

            decimal acumulado = 0m;
            foreach (var p2 in pagosValidos)
            {
                acumulado += p2.Monto;
                if (p2.PagoId == pagoId) break;   // nos detenemos al llegar a este pago
            }

            decimal montoTotal   = pago.Membresia.PrecioAcordado;
            decimal deudaRestante = Math.Max(0m, montoTotal - acumulado);

            return new PagoDetalleDTO
            {
                PagoId               = pago.PagoId,
                Monto                = pago.Monto,
                MetodoPago           = pago.MetodoPago,
                FechaPago            = pago.FechaPago,
                Comprobante          = pago.Comprobante,
                Observaciones        = pago.Observaciones,
                EsAnulado            = pago.EsAnulado,
                MembresiaId          = pago.MembresiaId,
                NombreCliente        = pago.Membresia.User.NombreCompleto,
                DniCliente           = pago.Membresia.User.Dni,
                NombreEmpleado       = pago.UsuarioEmpleado.NombreCompleto,
                PlanMembresia        = pago.Membresia.Plan.Nombre,
                MontoTotal           = montoTotal,
                MontoPagadoAcumulado = acumulado,
                DeudaRestante        = deudaRestante
            };
        }

        // ── ALMACENAMIENTO FÍSICO DE COMPROBANTES ──────────────────────────────
        /// <summary>
        /// Persiste la imagen del comprobante en disco y retorna la ruta relativa
        /// para almacenar en la base de datos. Retorna null si no hay comprobante.
        /// </summary>
        private async Task<string?> ProcesarComprobanteAsync(PagoCreateDTO dto, int empleadoId)
        {
            // Sin contenido → no hay comprobante que guardar
            bool tieneBase64 = !string.IsNullOrWhiteSpace(dto.ComprobanteBase64);
            bool tieneArchivo = dto.ComprobanteArchivo != null && dto.ComprobanteArchivo.Length > 0;

            if (!tieneBase64 && !tieneArchivo) return null;

            // Directorio destino: wwwroot/uploads/comprobantes
            var dirFisico = Path.Combine(_env.WebRootPath, "uploads", "comprobantes");
            Directory.CreateDirectory(dirFisico); // No-op si ya existe

            var nombreArchivo = $"yape_{empleadoId}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            var rutaFisica = Path.Combine(dirFisico, nombreArchivo);

            if (tieneBase64)
            {
                // Eliminar prefijo "data:image/jpeg;base64," u otros
                var base64Limpio = Regex.Replace(dto.ComprobanteBase64!, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
                var bytes = Convert.FromBase64String(base64Limpio);
                await File.WriteAllBytesAsync(rutaFisica, bytes);
            }
            else if (tieneArchivo)
            {
                using var stream = new FileStream(rutaFisica, FileMode.Create, FileAccess.Write);
                await dto.ComprobanteArchivo!.CopyToAsync(stream);
            }

            return "/uploads/comprobantes/" + nombreArchivo;
        }
    }
}