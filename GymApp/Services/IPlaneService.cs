using GymApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public interface IPlaneService
    {
        Task<IEnumerable<Plane>> ObtenerTodosAsync();
        Task<Plane> ObtenerPorIdAsync(int id);
        Task CrearPlanAsync(Plane plan);
        Task ActualizarPlanAsync(Plane plan);
        Task EliminarPlanAsync(int id);
    }
}