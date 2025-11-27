using GymApp.Data;
using GymApp.Models;

namespace GymApp.Repositories
{
    public class PlaneRepository : GenericRepository<Plane>, IPlaneRepository
    {
        public PlaneRepository(GymDbContext context) : base(context)
        {
        }
    }
}