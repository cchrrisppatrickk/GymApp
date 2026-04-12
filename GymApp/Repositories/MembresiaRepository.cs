using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    public class MembresiaRepository : GenericRepository<Membresia>, IMembresiaRepository
    {
        private readonly GymDbContext _context;

        public MembresiaRepository(GymDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Membresia>> ObtenerTodasConDetallesAsync()
        {
            // Eager Loading: Traemos las tablas relacionadas
            return await _context.Membresias
                .Include(m => m.User)
                .Include(m => m.Plan)
                .Include(m => m.Turno)
                .Include(m => m.PagosMembresia)
                .ToListAsync();
        }

        public async Task<Membresia?> ObtenerPorIdConDetallesAsync(int id)
        {
            return await _context.Membresias
                .Include(m => m.User)
                .Include(m => m.Plan)
                .Include(m => m.Turno)
                .Include(m => m.PagosMembresia)
                .Include(m => m.Congelamientos)
                    .ThenInclude(c => c.UsuarioEmpleado)
                .FirstOrDefaultAsync(m => m.MembresiaId == id);
        }



        // Implementación del nuevo método

        public async Task<Membresia?> GetLastActiveMembresiaByUserIdAsync(int userId)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            return await _context.Membresias
                .Include(m => m.Plan)   // <--- AGREGAR ESTO (Vital para saber el precio)
                .Include(m => m.User)   // <--- AGREGAR ESTO (Vital para saber el nombre)
                .Include(m => m.Turno)  // <--- AGREGAR ESTO (Buena práctica)
                .Where(m => m.UserId == userId && m.FechaVencimiento >= hoy)
                .OrderByDescending(m => m.MembresiaId)
                .FirstOrDefaultAsync();
        }
    }
}