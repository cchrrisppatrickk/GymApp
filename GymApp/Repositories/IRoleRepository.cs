using GymApp.Models;

namespace GymApp.Repositories
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        // Aquí podrías agregar métodos únicos, ej:
        // Task<Role> ObtenerPorNombreAsync(string nombre);
    }
}