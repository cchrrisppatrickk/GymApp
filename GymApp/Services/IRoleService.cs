using GymApp.Models;

namespace GymApp.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> ObtenerTodosAsync();
        Task<Role> ObtenerPorIdAsync(int id);
        Task CrearRolAsync(Role role);
        Task ActualizarRolAsync(Role role);
        Task<bool> EliminarRolAsync(int id); // Devuelve bool para saber si se pudo borrar
    }
}