using GymApp.Data;
using GymApp.Models;

namespace GymApp.Repositories
{
    public class ProductoRepository : GenericRepository<Producto>, IProductoRepository
    {
        public ProductoRepository(GymDbContext context) : base(context)
        {
        }
    }
}