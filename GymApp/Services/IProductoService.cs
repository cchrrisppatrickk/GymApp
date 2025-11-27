using GymApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public interface IProductoService
    {
        Task<IEnumerable<Producto>> ListarProductosAsync();
        Task<Producto> ObtenerPorIdAsync(int id);
        Task CrearProductoAsync(Producto producto);
        Task ActualizarProductoAsync(Producto producto);
        Task EliminarProductoAsync(int id);
    }
}