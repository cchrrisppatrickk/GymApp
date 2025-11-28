using GymApp.Models;
using GymApp.Repositories;
using GymApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public class MembresiaService : IMembresiaService
    {
        private readonly IMembresiaRepository _membresiaRepo;
        private readonly IPlaneRepository _planRepo;
        private readonly ITurnoRepository _turnoRepo;
        // Inyectamos repositorio de usuarios para la búsqueda
        private readonly IGenericRepository<Usuario> _usuarioRepo; 

        public MembresiaService(
            IMembresiaRepository membresiaRepo,
            IPlaneRepository planRepo,
            ITurnoRepository turnoRepo,
            IGenericRepository<Usuario> usuarioRepo)
        {
            _membresiaRepo = membresiaRepo;
            _planRepo = planRepo;
            _turnoRepo = turnoRepo;
            _usuarioRepo = usuarioRepo;
        }

        public async Task CrearMembresiaAsync(MembresiaCreateDTO dto)
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
                NombreUsuario = m.User.NombreCompleto,
                Dni = m.User.Dni,
                NombrePlan = m.Plan.Nombre,
                NombreTurno = m.Turno.Nombre,
                FechaInicio = m.FechaInicio.ToString("dd/MM/yyyy"),
                FechaVencimiento = m.FechaVencimiento.ToString("dd/MM/yyyy"),
                // Lógica visual de estado
                Estado = m.FechaVencimiento < hoy ? "Vencida" : "Activa",
                DiasRestantes = m.FechaVencimiento.DayNumber - hoy.DayNumber
            });

            // Aplicar Filtros
            if (filtro == "vencidas")
                lista = lista.Where(x => x.DiasRestantes < 0);
            else if (filtro == "por_vencer")
                lista = lista.Where(x => x.DiasRestantes >= 0 && x.DiasRestantes <= 5);
            else if (filtro == "activas")
                lista = lista.Where(x => x.DiasRestantes >= 0);

            return lista;
        }

        public async Task<IEnumerable<object>> BuscarClientesAsync(string termino)
        {
            var usuarios = await _usuarioRepo.GetAllAsync();
            // Filtro simple en memoria (para producción idealmente se filtra en BD)
            return usuarios
                .Where(u => u.NombreCompleto.Contains(termino, StringComparison.OrdinalIgnoreCase) || u.Dni.Contains(termino))
                .Take(10)
                .Select(u => new { id = u.UserId, text = $"{u.NombreCompleto} ({u.Dni})" });
        }

        // *** NUEVO MÉTODO DE VALIDACIÓN DE TURNO ***
        // Valida que un cliente no pueda tener dos membresías activas con diferentes turnos.
        public async Task VerificarTurnoExistente(int userId, int nuevoTurnoId)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            // Buscar cualquier membresía activa (no vencida) para este usuario
            var membresiasActivas = await _membresiaRepo.GetAllAsync();
            var activa = membresiasActivas
                            .Where(m => m.UserId == userId && m.FechaVencimiento >= hoy)
                            .FirstOrDefault();

            if (activa != null && activa.TurnoId != nuevoTurnoId)
            {
                // Si existe una membresía activa y el nuevo turno es diferente, lanzamos una excepción de negocio.
                throw new Exception($"El cliente ya tiene una membresía activa (vence {activa.FechaVencimiento:dd/MM/yyyy}) con el turno {activa.Turno.Nombre}. Para cambiar de turno, debe esperar a que venza o anular la anterior.");
            }
            // Si no tiene activa, o si la activa tiene el mismo turno, se procede sin error.
        }

    }
}