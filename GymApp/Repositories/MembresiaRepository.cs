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
                .OrderByDescending(m => m.MembresiaId) // Las más nuevas primero
                .ToListAsync();
        }
    }
}