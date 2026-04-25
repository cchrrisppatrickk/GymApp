using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    public class PagoRepository : GenericRepository<PagosMembresium>, IPagoRepository
    {
        private readonly GymDbContext _context;

        public PagoRepository(GymDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PagosMembresium>> ObtenerHistorialCompletoAsync()
        {
            return await _context.PagosMembresia
                .Include(p => p.Membresia).ThenInclude(m => m.User) // Cliente
                .Include(p => p.Membresia).ThenInclude(m => m.Plan) // Plan
                .Include(p => p.UsuarioEmpleado) // Empleado que cobró
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        //nuevo
        public async Task<decimal> GetTotalPagadoAsync(int membresiaId)
        {
            return await _context.PagosMembresia
                .Where(p => p.MembresiaId == membresiaId && !p.EsAnulado)
                .SumAsync(p => p.Monto);
        }
    }
}