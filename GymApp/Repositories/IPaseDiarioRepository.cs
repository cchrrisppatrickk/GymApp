using System.Collections.Generic;
using System.Threading.Tasks;
using GymApp.Models;

namespace GymApp.Repositories;

public interface IPaseDiarioRepository : IGenericRepository<PaseDiario>
{
    Task<IEnumerable<PaseDiario>> ObtenerTodosConDetallesAsync();
}
