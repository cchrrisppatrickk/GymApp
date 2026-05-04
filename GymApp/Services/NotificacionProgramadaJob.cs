using System;
using System.Linq;
using System.Threading.Tasks;
using GymApp.Repositories;

namespace GymApp.Services
{
    public class NotificacionProgramadaJob
    {
        private readonly IConfiguracionAlertaRepository _configRepo;
        private readonly IReporteService _reporteService;
        private readonly IUsuarioService _usuarioService;
        private readonly IWebhookService _webhookService;

        public NotificacionProgramadaJob(
            IConfiguracionAlertaRepository configRepo,
            IReporteService reporteService,
            IUsuarioService usuarioService,
            IWebhookService webhookService)
        {
            _configRepo = configRepo;
            _reporteService = reporteService;
            _usuarioService = usuarioService;
            _webhookService = webhookService;
        }

        public async Task EjecutarRevisionAsync()
        {
            var configuraciones = await _configRepo.GetAllAsync();
            var configActivas = configuraciones.Where(c => c.Activo).ToList();

            var horaActual = DateTime.Now.TimeOfDay;
            // DayOfWeek.Sunday = 0, Monday = 1 ... 
            var diaActual = ((int)DateTime.Now.DayOfWeek).ToString();

            foreach (var config in configActivas)
            {
                // Solo ejecutar si es la misma hora y minuto
                if (config.HoraEnvio.Hours == horaActual.Hours && config.HoraEnvio.Minutes == horaActual.Minutes)
                {
                    // Validar día si la configuración lo requiere
                    if (string.IsNullOrEmpty(config.DiasSemana) || config.DiasSemana.Split(',').Contains(diaActual))
                    {
                        object miembros = null;
                        object pagos = null;
                        object deudas = null;

                        if (config.EnviarNuevosMiembros)
                        {
                            miembros = await _usuarioService.ObtenerRecientesParaAgenteAsync(1); // Último día
                        }

                        if (config.EnviarPagosHoy)
                        {
                            // Aproximación: pagos recientes
                            pagos = await _reporteService.ObtenerPagosRecientesAsync(20); 
                        }

                        if (config.EnviarDeudasPendientes)
                        {
                            deudas = await _reporteService.ObtenerListaDeudoresAsync();
                        }

                        var resumen = new
                        {
                            NuevosMiembros = miembros,
                            PagosRecientes = pagos,
                            Deudores = deudas
                        };

                        await _webhookService.EnviarReporteProgramadoAsync(resumen, config.ChatIdDestino);
                    }
                }
            }
        }
    }
}
