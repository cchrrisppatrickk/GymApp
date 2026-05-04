using GymApp.Data;
using GymApp.Models;

namespace GymApp.Repositories
{
    public class ConfiguracionAlertaRepository : GenericRepository<ConfiguracionAlerta>, IConfiguracionAlertaRepository
    {
        public ConfiguracionAlertaRepository(GymDbContext context) : base(context)
        {
        }
    }
}
