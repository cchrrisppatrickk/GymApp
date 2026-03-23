using GymApp.Models;
using GymApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public interface ICongelamientoService
    {
        // Ejecuta el proceso de congelar una membresía
        Task CongelarAsync(CongelarMembresiaDTO dto);

        // Retorna el historial de congelamientos de una membresía específica
        Task<IEnumerable<Congelamiento>> ListarHistorialAsync(int membresiaId);

        // Descongela manualmente una membresía que todavía tiene tiempo de suspensión
        Task DescongelarManualAsync(int membresiaId);
    }
}
