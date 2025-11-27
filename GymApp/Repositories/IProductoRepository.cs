using GymApp.Models;

namespace GymApp.Repositories
{
    public interface IProductoRepository : IGenericRepository<Producto>
    {
        // Aquí pondríamos métodos futuros como: 
        // Task<IEnumerable<Producto>> ObtenerProductosBajosDeStockAsync(int limite);
    }
}