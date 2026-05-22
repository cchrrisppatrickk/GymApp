using System.Collections.Generic;
using System.Threading.Tasks;
using GymApp.ViewModels;

namespace GymApp.Services;

public interface IPaseDiarioService
{
    Task RegistrarPaseAsync(PaseDiarioCreateDTO dto, int empleadoId);
    Task<IEnumerable<PaseDiarioListDTO>> ListarPasesAsync();
    Task EliminarFisicamenteAsync(int id);
}
