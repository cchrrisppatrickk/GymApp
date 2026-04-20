using GymApp.ViewModels;
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
    }
}