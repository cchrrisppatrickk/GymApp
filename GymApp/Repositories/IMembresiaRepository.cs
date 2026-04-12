using GymApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    public interface IMembresiaRepository : IGenericRepository<Membresia>
    {
        // Trae la membresía con sus relaciones (Include)
        Task<IEnumerable<Membresia>> ObtenerTodasConDetallesAsync();

        // Obtiene una membresía específica con todos sus detalles (pagos, congelamientos, etc.)
        Task<Membresia?> ObtenerPorIdConDetallesAsync(int id);

        // Nuevo: Obtiene la última membresía activa/no vencida de un usuario.
        Task<Membresia?> GetLastActiveMembresiaByUserIdAsync(int userId);
    }
}