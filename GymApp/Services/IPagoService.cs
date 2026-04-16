using GymApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public interface IPagoService
    {
        Task<IEnumerable<PagoListDTO>> ListarPagosAsync();
        Task<int> RegistrarPagoAsync(PagoCreateDTO dto, int empleadoId);
        Task<List<DeudaInfoDTO>> BuscarDeudaClienteAsync(string termino);
    }
}