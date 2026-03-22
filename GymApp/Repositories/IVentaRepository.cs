using GymApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    public interface IVentaRepository : IGenericRepository<VentasCabecera>
    {
        Task<IEnumerable<VentasCabecera>> ObtenerHistorialCompletoAsync();
    }
}