using GymApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public interface ITurnoService
    {
        Task<IEnumerable<Turno>> ObtenerTodosAsync();
        Task<Turno> ObtenerPorIdAsync(int id);
        Task CrearTurnoAsync(Turno turno);
        Task ActualizarTurnoAsync(Turno turno);
        Task EliminarTurnoAsync(int id);
    }
}