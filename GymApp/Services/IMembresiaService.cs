using GymApp.Models;
using GymApp.ViewModels;
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
        Task<PagedResult<MembresiaListDTO>> ObtenerMembresiasPaginadasAsync(string? buscar, int? mes, int? anio, int pagina, int tamanoPagina = 20);
    }
}