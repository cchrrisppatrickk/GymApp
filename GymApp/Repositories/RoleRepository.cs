using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        private readonly GymDbContext _context;

        public RoleRepository(GymDbContext context) : base(context)
        {
            _context = context;
        }

        // Aquí implementarías los métodos específicos de IRoleRepository si los hubiera
    }
}