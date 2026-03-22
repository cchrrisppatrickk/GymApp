using GymApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    public interface IMembresiaRepository : IGenericRepository<Membresia>
    {
        // Trae la membresía con sus relaciones (Include)
        Task<IEnumerable<Membresia>> ObtenerTodasConDetallesAsync();

        // Nuevo: Obtiene la última membresía activa/no vencida de un usuario.
        Task<Membresia?> GetLastActiveMembresiaByUserIdAsync(int userId);
    }
}