using GymApp.ViewModels;
using GymApp.ViewModels.ApiAgent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public interface IPagoService
    {
        Task<IEnumerable<PagoListDTO>> ListarPagosAsync();
        Task<PagedResult<PagoListDTO>> ObtenerPagosPaginadosAsync(string? buscar, int? mes, int? anio, int pagina, int tamanoPagina = 20);
        Task<int> RegistrarPagoAsync(PagoCreateDTO dto, int empleadoId);
        Task<List<DeudaInfoDTO>> BuscarDeudaClienteAsync(string termino);

        // --- Para ApiAgent ---
        Task<DeudaUsuarioAgenteDTO> ObtenerDeudaTotalParaAgenteAsync(int userId);
        Task<IEnumerable<PagoAgenteDTO>> ObtenerHistorialUsuarioParaAgenteAsync(int userId);
        Task<IEnumerable<PagoAgenteDTO>> ObtenerPagosPorRangoParaAgenteAsync(DateTime inicio, DateTime fin);
    }
}