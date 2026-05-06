using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    public class ConfiguracionAlertaRepository : GenericRepository<ConfiguracionAlerta>, IConfiguracionAlertaRepository
    {
        public ConfiguracionAlertaRepository(GymDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ConfiguracionAlerta>> ObtenerAlertasParaEjecutarAsync(TimeSpan horaActual, string diaSemana)
        {
            var horaComparar = new TimeSpan(horaActual.Hours, horaActual.Minutes, 0);

            return await _context.ConfiguracionAlertas
                .Where(c => c.Activo &&
                            c.HoraEnvio.Hours == horaComparar.Hours &&
                            c.HoraEnvio.Minutes == horaComparar.Minutes &&
                            c.DiasSemana.Contains(diaSemana))
                .ToListAsync();
        }
    }
}
