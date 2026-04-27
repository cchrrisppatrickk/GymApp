using GymApp.Models;
using GymApp.ViewModels;
using GymApp.ViewModels.ApiAgent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public interface IMembresiaService
    {
        Task<IEnumerable<MembresiaListDTO>> ListarMembresiasAsync(string filtro);
        Task<int> CrearMembresiaAsync(MembresiaCreateDTO dto);
        Task<IEnumerable<object>> BuscarClientesAsync(string termino); // Para el autocomplete

        Task<Membresia> ObtenerDetallesAsync(int id);
        Task<DateOnly> ObtenerPropuestaRenovacionAsync(int membresiaId);

        // Nuevo: Verifica si el Turno seleccionado ya está asignado a otra membresía activa
        Task VerificarTurnoExistente(int userId, int turnoId);

        Task<bool> CongelarMembresiaAsync(int membresiaId, int empleadoId, DateOnly fechaFin, string motivo);

        Task<bool> TieneMembresiaActivaAsync(int userId);
        Task<bool> TieneRenovacionProgramadaAsync(int userId);
        Task<bool> EditarMembresiaAsync(MembresiaEditDTO dto);
        Task<PagedResult<MembresiaListDTO>> ObtenerMembresiasPaginadasAsync(string? buscar, int? mes, int? anio, int pagina, string? filtro = null, int tamanoPagina = 20);

        // ── Dominio de Membresías — Consultas granulares para el Agente IA ──────
        /// <summary>Devuelve la membresía activa o congelada actual del usuario. Null si no tiene.</summary>
        Task<MembresiaAgenteDTO?> ObtenerActivaParaAgenteAsync(int userId);

        /// <summary>Devuelve el historial completo de membresías del usuario, más reciente primero.</summary>
        Task<IEnumerable<MembresiaAgenteDTO>> ObtenerHistorialParaAgenteAsync(int userId);

        /// <summary>
        /// Devuelve membresías en estado crítico: activas con N días o menos hasta vencer,
        /// y vencidas en los últimos 15 días.
        /// </summary>
        Task<IEnumerable<MembresiaAgenteDTO>> ObtenerAlertasParaAgenteAsync(int diasPorVencer);
        Task EliminarMembresiaFisicamenteAsync(int membresiaId);
    }
}