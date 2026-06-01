using GymApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public interface IRestriccionService
    {
        Task<IEnumerable<RestriccionUsuario>> ObtenerPorUsuarioAsync(int userId);
        Task<RestriccionUsuario> AplicarRestriccionAsync(int userId, string tipo, string descripcion, int usuarioAplicadorId);
        Task<bool> LevantarRestriccionAsync(int restriccionId);
        Task<bool> UsuarioTieneRestriccionesActivasAsync(int userId);
    }
}
