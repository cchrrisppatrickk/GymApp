using GymApp.Models;

namespace GymApp.Repositories
{
    public interface ICongelamientoRepository : IGenericRepository<Congelamiento>
    {
        // Obtiene todos los congelamientos de una membresía específica con sus detalles
        Task<IEnumerable<Congelamiento>> GetHistorialPorMembresiaAsync(int membresiaId);
        
        // Verifica si existe un congelamiento activo para una membresía hoy
        Task<Congelamiento?> GetCongelamientoActivoAsync(int membresiaId);
    }
}
