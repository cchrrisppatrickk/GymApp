using GymApp.Models;
using GymApp.Repositories;
using GymApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GymApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services
{
    public class MembresiaService : IMembresiaService
    {
        private readonly IMembresiaRepository _membresiaRepo;
        private readonly IPlaneRepository _planRepo;
        private readonly ITurnoRepository _turnoRepo;
        // Inyectamos repositorio de usuarios para la búsqueda
        private readonly IGenericRepository<Usuario> _usuarioRepo;
        private readonly IGenericRepository<Congelamiento> _congelamientoRepo;
        private readonly GymDbContext _context;

        public MembresiaService(
            IMembresiaRepository membresiaRepo,
            IPlaneRepository planRepo,
            ITurnoRepository turnoRepo,
            IGenericRepository<Usuario> usuarioRepo,
            IGenericRepository<Congelamiento> congelamientoRepo,
            GymDbContext context)
        {
            _membresiaRepo = membresiaRepo;
            _planRepo = planRepo;
            _turnoRepo = turnoRepo;
            _usuarioRepo = usuarioRepo;
            _congelamientoRepo = congelamientoRepo;
            _context = context;
        }

        public async Task<int> CrearMembresiaAsync(MembresiaCreateDTO dto)
        {
            // 1. Validaciones Preliminares (Lógica de Negocio)
            await VerificarTurnoExistente(dto.UserId, dto.TurnoId);

            var plan = await _planRepo.GetByIdAsync(dto.PlanId);
            if (plan == null) throw new Exception("Plan no encontrado.");

            // 2. Lógica de Renovación Inteligente (El cambio clave)
            var ultimaMembresiaActiva = await _membresiaRepo.GetLastActiveMembresiaByUserIdAsync(dto.UserId);

            DateOnly nuevaFechaInicio;

            if (ultimaMembresiaActiva != null)
            {
                // **Caso de Renovación:** El plan anterior aún no ha vencido.
                // La nueva membresía empieza 1 día después del vencimiento anterior.
                // IMPORTANTE: Esto evita que el cliente pierda días o que el nuevo plan se solape.
                nuevaFechaInicio = ultimaMembresiaActiva.FechaVencimiento.AddDays(1);
            }
            else
            {
                // **Caso de Nueva Membresía o Membresía Vencida:** // La membresía empieza en la fecha seleccionada (normalmente hoy).
                nuevaFechaInicio = DateOnly.FromDateTime(dto.FechaInicio);
            }

            // 3. Calcular Fecha de Vencimiento
            var nuevaFechaFin = nuevaFechaInicio.AddDays(plan.DuracionDias);

            // 4. Crear Entidad
            var nuevaMembresia = new Membresia
            {
                UserId = dto.UserId,
                PlanId = dto.PlanId,
                TurnoId = dto.TurnoId,
                FechaInicio = nuevaFechaInicio,
                FechaVencimiento = nuevaFechaFin,
                Estado = "Pendiente Pago"
            };

            await _membresiaRepo.InsertAsync(nuevaMembresia);
            await _membresiaRepo.SaveAsync();

            return nuevaMembresia.MembresiaId;
        }

        public async Task<IEnumerable<MembresiaListDTO>> ListarMembresiasAsync(string filtro)
        {
            var rawData = await _membresiaRepo.ObtenerTodasConDetallesAsync();
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var cincoDias = hoy.AddDays(5);

            // Mapeo manual a DTO
            var lista = rawData.Select(m => new MembresiaListDTO
            {
                MembresiaId = m.MembresiaId,
                UserId = m.UserId,
                NombreUsuario = m.User.NombreCompleto,
                Dni = m.User.Dni,
                NombrePlan = m.Plan.Nombre,
                NombreTurno = m.Turno.Nombre,
                FechaInicio = m.FechaInicio.ToString("dd/MM/yyyy"),
                FechaVencimiento = m.FechaVencimiento.ToString("dd/MM/yyyy"),
                // Lógica visual de estado
                Estado = m.FechaVencimiento < hoy ? "Vencida" : m.Estado,
                DiasRestantes = m.FechaVencimiento.DayNumber - hoy.DayNumber,
                DiasVencidos = m.FechaVencimiento < hoy ? hoy.DayNumber - m.FechaVencimiento.DayNumber : 0,
                PermiteCongelar = m.Plan.PermiteCongelar ?? false,
                Deuda = (m.Plan?.PrecioBase ?? 0m) - (m.PagosMembresia?.Sum(p => p.Monto) ?? 0m)
            });

            // Aplicar Filtros
            if (filtro == "vencidas")
                lista = lista.Where(x => x.DiasRestantes < 0);
            else if (filtro == "por_vencer")
                lista = lista.Where(x => x.DiasRestantes >= 0 && x.DiasRestantes <= 5);
            else if (filtro == "activas")
                lista = lista.Where(x => x.DiasRestantes >= 0);
            else if (filtro == "congeladas")
                lista = lista.Where(x => x.Estado == "Congelada");

            return lista;
        }

        public async Task<PagedResult<MembresiaListDTO>> ObtenerMembresiasPaginadasAsync(string? buscar, int? mes, int? anio, int pagina, int tamanoPagina = 20)
        {
            var query = _context.Membresias
                .Include(m => m.User)
                .Include(m => m.Plan)
                .Include(m => m.Turno)
                .Include(m => m.PagosMembresia)
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
            {
                var termino = buscar.ToLower();
                query = query.Where(m => m.User.NombreCompleto.ToLower().Contains(termino) || 
                                         (m.User.Dni != null && m.User.Dni.Contains(termino)));
            }
            else if (mes.HasValue && anio.HasValue)
            {
                query = query.Where(m => m.FechaInicio.Month == mes.Value && m.FechaInicio.Year == anio.Value);
            }

            // Orden normal
            query = query.OrderByDescending(m => m.MembresiaId);

            int totalRegistros = await query.CountAsync();
            var items = await query.Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina).ToListAsync();
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var listaDto = items.Select(m => new MembresiaListDTO
            {
                MembresiaId = m.MembresiaId,
                UserId = m.UserId,
                NombreUsuario = m.User.NombreCompleto,
                Dni = m.User.Dni,
                NombrePlan = m.Plan.Nombre,
                NombreTurno = m.Turno.Nombre,
                FechaInicio = m.FechaInicio.ToString("dd/MM/yyyy"),
                FechaVencimiento = m.FechaVencimiento.ToString("dd/MM/yyyy"),
                Estado = m.FechaVencimiento < hoy ? "Vencida" : m.Estado,
                DiasRestantes = m.FechaVencimiento.DayNumber - hoy.DayNumber,
                DiasVencidos = m.FechaVencimiento < hoy ? hoy.DayNumber - m.FechaVencimiento.DayNumber : 0,
                PermiteCongelar = m.Plan.PermiteCongelar ?? false,
                Deuda = (m.Plan?.PrecioBase ?? 0m) - (m.PagosMembresia?.Sum(p => p.Monto) ?? 0m)
            }).ToList();

            return new PagedResult<MembresiaListDTO>
            {
                Items = listaDto,
                TotalPages = (int)Math.Ceiling((double)totalRegistros / tamanoPagina),
                CurrentPage = pagina,
                SearchTerm = buscar
            };
        }

        public async Task<Membresia> ObtenerDetallesAsync(int id)
        {
            var membresia = await _membresiaRepo.ObtenerPorIdConDetallesAsync(id);
            if (membresia == null) throw new Exception("Membresía no encontrada.");
            return membresia;
        }

        public async Task<IEnumerable<object>> BuscarClientesAsync(string termino)

        {
            var usuarios = await _usuarioRepo.GetAllAsync();
            // Filtro simple en memoria (para producción idealmente se filtra en BD)
            return usuarios
                .Where(u => u.NombreCompleto.Contains(termino, StringComparison.OrdinalIgnoreCase) || (u.Dni != null && u.Dni.Contains(termino)))
                .Take(10)
                .Select(u => new { id = u.UserId, text = u.NombreCompleto + (u.Dni != null ? $" (DNI: {u.Dni})" : " (Sin DNI)") });
        }

        // *** NUEVO MÉTODO DE VALIDACIÓN DE TURNO ***
        // Valida que un cliente no pueda tener dos membresías activas con diferentes turnos.
        public async Task VerificarTurnoExistente(int userId, int nuevoTurnoId)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            // Cargamos con detalles para tener el nombre del turno si es necesario
            var todas = await _membresiaRepo.ObtenerTodasConDetallesAsync();
            var activa = todas.FirstOrDefault(m => m.UserId == userId && m.FechaVencimiento >= hoy);

            if (activa != null && activa.TurnoId != nuevoTurnoId)
            {
                // Si existe una membresía activa y el nuevo turno es diferente, lanzamos una excepción de negocio.
                throw new Exception($"El cliente ya tiene una membresía activa (vence {activa.FechaVencimiento:dd/MM/yyyy}) con el turno {activa.Turno?.Nombre}. Para cambiar de turno, debe esperar a que venza o anular la anterior.");
            }
            // Si no tiene activa, o si la activa tiene el mismo turno, se procede sin error.
        }

        public async Task<bool> EditarMembresiaAsync(MembresiaEditDTO dto)
        {
            var membresia = await _membresiaRepo.GetByIdAsync(dto.MembresiaId);
            if (membresia == null) throw new Exception("Membresía no encontrada.");

            membresia.PlanId = dto.PlanId;
            membresia.TurnoId = dto.TurnoId;
            membresia.FechaInicio = DateOnly.FromDateTime(dto.FechaInicio);
            membresia.FechaVencimiento = DateOnly.FromDateTime(dto.FechaVencimiento);

            if (membresia.FechaVencimiento < DateOnly.FromDateTime(DateTime.Now))
            {
                membresia.Estado = "Vencida";
            }
            else
            {
                membresia.Estado = "Activa";
            }

            await _membresiaRepo.UpdateAsync(membresia);
            await _membresiaRepo.SaveAsync();

            return true;
        }

        public async Task<DateOnly> ObtenerPropuestaRenovacionAsync(int membresiaId)
        {
            var membresia = await _membresiaRepo.GetByIdAsync(membresiaId);
            if (membresia == null) return DateOnly.FromDateTime(DateTime.Today);

            var hoy = DateOnly.FromDateTime(DateTime.Today);
            
            // Si vence en el futuro, la renovación empieza el día siguiente al vencimiento.
            if (membresia.FechaVencimiento > hoy)
            {
                return membresia.FechaVencimiento.AddDays(1);
            }
            
            // Si ya venció, empieza hoy.
            return hoy;
        }

        public async Task<bool> CongelarMembresiaAsync(int membresiaId, int empleadoId, DateOnly fechaFin, string motivo)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var membresia = await _membresiaRepo.ObtenerPorIdConDetallesAsync(membresiaId);

            if (membresia == null) throw new Exception("Membresía no encontrada.");
            
            // 1. Validar que el plan permita congelar
            if (membresia.Plan?.PermiteCongelar != true)
                throw new Exception("Este plan no permite congelamientos.");

            // 2. Validar que esté activa
            if (membresia.FechaVencimiento <= hoy)
                throw new Exception("No se puede congelar una membresía ya vencida.");

            if (fechaFin <= hoy)
                throw new Exception("La fecha de fin debe ser futura.");

            // 3. Calcular días a extender
            int diasExtra = fechaFin.DayNumber - hoy.DayNumber;

            // 4. Crear registro de congelamiento
            var congelamiento = new Congelamiento
            {
                MembresiaId = membresiaId,
                UsuarioEmpleadoId = empleadoId,
                FechaInicio = hoy,
                FechaFin = fechaFin,
                Motivo = motivo,
                FechaRegistro = DateTime.Now
            };

            // 5. Actualizar Membresía
            membresia.FechaVencimiento = membresia.FechaVencimiento.AddDays(diasExtra);
            membresia.Estado = "Congelada";

            await _congelamientoRepo.InsertAsync(congelamiento);
            await _membresiaRepo.UpdateAsync(membresia);
            
            await _membresiaRepo.SaveAsync();
            return true;
        }

        public async Task<bool> TieneMembresiaActivaAsync(int userId)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var todas = await _membresiaRepo.GetAllAsync();
            return todas.Any(m => m.UserId == userId && m.FechaVencimiento >= hoy && m.Estado == "Activa");
        }

        public async Task<bool> TieneRenovacionProgramadaAsync(int userId)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var todas = await _membresiaRepo.GetAllAsync();
            return todas.Any(m => m.UserId == userId && m.FechaInicio > hoy && m.Estado == "Activa");
        }
    }
}
