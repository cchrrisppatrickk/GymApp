using GymApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    public interface IConfiguracionAlertaRepository
    {
        Task<ConfiguracionAlerta> ObtenerConfiguracionGlobalAsync();
        Task<bool> GuardarConfiguracionGlobalAsync(ConfiguracionAlerta configuracion);
        Task<IEnumerable<ConfiguracionAlerta>> ObtenerAlertasParaEjecutarAsync(TimeSpan horaActual, string diaSemana);
    }
}
