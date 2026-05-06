using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    public class ConfiguracionAlertaRepository : IConfiguracionAlertaRepository
    {
        private readonly GymDbContext _context;

        public ConfiguracionAlertaRepository(GymDbContext context)
        {
            _context = context;
        }

        public async Task<ConfiguracionAlerta> ObtenerConfiguracionGlobalAsync()
        {
            var config = await _context.ConfiguracionAlertas.FirstOrDefaultAsync(c => c.Id == 1);
            if (config == null)
            {
                // Retornar objeto nuevo (Singleton en memoria si no existe en DB)
                config = new ConfiguracionAlerta { Id = 1, Activo = false };
            }
            return config;
        }

        public async Task<bool> GuardarConfiguracionGlobalAsync(ConfiguracionAlerta configuracion)
        {
            configuracion.Id = 1; // Forzar Id 1
            var existe = await _context.ConfiguracionAlertas.AnyAsync(c => c.Id == 1);

            if (existe)
            {
                _context.ConfiguracionAlertas.Update(configuracion);
            }
            else
            {
                await _context.ConfiguracionAlertas.AddAsync(configuracion);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<ConfiguracionAlerta>> ObtenerAlertasParaEjecutarAsync(TimeSpan horaActual, string diaSemana)
        {
            var horaComparar = new TimeSpan(horaActual.Hours, horaActual.Minutes, 0);

            return await _context.ConfiguracionAlertas
                .Where(c => c.Id == 1 && c.Activo &&
                            c.HoraEnvio.Hours == horaComparar.Hours &&
                            c.HoraEnvio.Minutes == horaComparar.Minutes &&
                            c.DiasSemana.Contains(diaSemana))
                .ToListAsync();
        }
    }
}
