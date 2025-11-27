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
            // 1. Obtener datos del Plan para saber la duración
            var plan = await _planRepo.GetByIdAsync(dto.PlanId);
            if (plan == null) throw new Exception("Plan no encontrado");

            // 2. Calcular Fechas
            var fechaInicio = dto.FechaInicio;
            var fechaFin = fechaInicio.AddDays(plan.DuracionDias);

            // 3. Crear Entidad
            var nuevaMembresia = new Membresia
            {
                UserId = dto.UserId,
                PlanId = dto.PlanId,
                TurnoId = dto.TurnoId,
                FechaInicio = DateOnly.FromDateTime(fechaInicio), // Asumiendo .NET 6/8
                FechaVencimiento = DateOnly.FromDateTime(fechaFin),
                Estado = "Activa" // Estado inicial
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
    }
}