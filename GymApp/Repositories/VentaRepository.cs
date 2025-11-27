using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    public class VentaRepository : GenericRepository<VentasCabecera>, IVentaRepository
    {
        private readonly GymDbContext _context;

        public VentaRepository(GymDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VentasCabecera>> ObtenerHistorialCompletoAsync()
        {
            return await _context.VentasCabeceras
                .Include(v => v.User) // Cliente
                .Include(v => v.UsuarioEmpleado) // Quién vendió
                .Include(v => v.VentasDetalles)
                    .ThenInclude(d => d.Producto) // Para saber qué productos llevó
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }
    }
}