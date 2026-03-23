using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Repositories
{
    public class CongelamientoRepository : GenericRepository<Congelamiento>, ICongelamientoRepository
    {
        private readonly GymDbContext _context;

        public CongelamientoRepository(GymDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Congelamiento>> GetHistorialPorMembresiaAsync(int membresiaId)
        {
            return await _context.Congelamientos
                .Include(c => c.UsuarioEmpleado) // Traer quién autorizó
                .Where(c => c.MembresiaId == membresiaId)
                .OrderByDescending(c => c.FechaInicio)
                .ToListAsync();
        }

        public async Task<Congelamiento?> GetCongelamientoActivoAsync(int membresiaId)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            
            return await _context.Congelamientos
                .Where(c => c.MembresiaId == membresiaId && 
                            c.FechaInicio <= hoy && 
                            c.FechaFin >= hoy)
                .FirstOrDefaultAsync();
        }
    }
}
