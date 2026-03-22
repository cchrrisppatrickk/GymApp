using GymApp.Models;

namespace GymApp.Repositories
{
    public interface IPlaneRepository : IGenericRepository<Plane>
    {
        // Aquí podrías agregar métodos futuros, ej:
        // Task<IEnumerable<Plane>> ObtenerPlanesConCongelamientoAsync();
    }
}