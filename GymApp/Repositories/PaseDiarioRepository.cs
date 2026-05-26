using System.Collections.Generic;
using System.Threading.Tasks;
using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Repositories;

public class PaseDiarioRepository : GenericRepository<PaseDiario>, IPaseDiarioRepository
{
    public PaseDiarioRepository(GymDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PaseDiario>> ObtenerTodosConDetallesAsync()
    {
        return await _context.PasesDiarios
            .Include(p => p.User)
            .Include(p => p.Turno)
            .Include(p => p.UsuarioEmpleado)
            .ToListAsync();
    }

    public async Task<PaseDiario?> ObtenerPorIdConDetallesAsync(int id)
    {
        return await _context.PasesDiarios
            .Include(p => p.User)
            .Include(p => p.Turno)
            .Include(p => p.UsuarioEmpleado)
            .FirstOrDefaultAsync(p => p.PaseDiarioId == id);
    }
}
