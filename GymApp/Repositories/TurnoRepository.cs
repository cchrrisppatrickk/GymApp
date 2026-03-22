using GymApp.Data;
using GymApp.Models;

namespace GymApp.Repositories
{
    public class TurnoRepository : GenericRepository<Turno>, ITurnoRepository
    {
        public TurnoRepository(GymDbContext context) : base(context)
        {
            // El constructor pasa el contexto a la clase base GenericRepository
        }
    }
}