using GymApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    public interface IPagoRepository : IGenericRepository<PagosMembresium>
    {
        Task<IEnumerable<PagosMembresium>> ObtenerHistorialCompletoAsync();
    }
}