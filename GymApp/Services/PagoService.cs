using GymApp.Models;
using GymApp.Repositories;
using GymApp.ViewModels;
using GymApp.ViewModels.ApiAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services
{
    public class PagoService : IPagoService
    {
        private readonly IPagoRepository _pagoRepo;
        private readonly IMembresiaRepository _membresiaRepo;
        private readonly IUsuarioRepository _usuarioRepo; // Necesitamos buscar user por DNI
        private readonly GymDbContext _context;

        public PagoService(IPagoRepository pagoRepo, IMembresiaRepository membresiaRepo, IUsuarioRepository usuarioRepo, GymDbContext context)
        {
            _pagoRepo = pagoRepo;
            _membresiaRepo = membresiaRepo;
            _usuarioRepo = usuarioRepo;
            _context = context;
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
                decimal yaPagado = await _pagoRepo.GetTotalPagadoAsync(membresia.MembresiaId);
                decimal deuda = precioAcordado - yaPagado;

                resultados.Add(new DeudaInfoDTO
                {
                    MembresiaId = membresia.MembresiaId,
                    NombreCliente = membresia.User.NombreCompleto,
                    DniCliente = membresia.User.Dni,
                    NombrePlan = membresia.Plan.Nombre,
                    Estado = membresia.FechaVencimiento < DateOnly.FromDateTime(DateTime.Now) && membresia.Estado != "Pendiente Pago" ? "Vencida" : membresia.Estado,
                    PrecioTotal = precioAcordado,
                    TotalPagado = yaPagado,
                    DeudaPendiente = deuda > 0 ? deuda : 0
                });
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

            // 2. Registrar el Pago (Inmutable)
            var nuevoPago = new PagosMembresium
            {
                MembresiaId = dto.MembresiaId,
                UsuarioEmpleadoId = empleadoId,
                Monto = dto.Monto,
                MetodoPago = dto.MetodoPago,
                FechaPago = dto.FechaPago ?? DateTime.Now,
                Comprobante = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
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

            return nuevoPago.PagoId;
        }

        public async Task ActualizarPagoAsync(PagoEditDTO dto, int empleadoId)
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
        }

        public async Task AnularPagoAsync(int id, string motivoAnulacion, int empleadoId)
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

            decimal otrosPagosValidos = pago.Membresia.PagosMembresia
                .Where(p => !p.EsAnulado)
                .Sum(p => p.Monto);

            decimal deudaResultante = pago.Membresia.PrecioAcordado - otrosPagosValidos;

            if (deudaResultante > 0 && pago.Membresia.Estado == "Activa")
            {
                // Si vuelve a tener deuda, regresa a Pendiente Pago
                pago.Membresia.Estado = "Pendiente Pago";
            }

            _context.PagosMembresia.Update(pago);
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
    }
}