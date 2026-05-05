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
            var horaActual = DateTime.Now.TimeOfDay;
            // DayOfWeek.Sunday = 0, Monday = 1 ... 
            var diaActual = ((int)DateTime.Now.DayOfWeek).ToString();

            var alertasPendientes = await _configRepo.ObtenerAlertasParaEjecutarAsync(horaActual, diaActual);

            foreach (var alerta in alertasPendientes)
            {
                object miembros = null;
                object pagos = null;
                object deudas = null;
                object vencimientos = null;

                if (alerta.EnviarNuevosMiembros)
                {
                    miembros = await _usuarioService.ObtenerRecientesParaAgenteAsync(1); // Último día
                }

                if (alerta.EnviarPagosHoy)
                {
                    // Aproximación: pagos recientes
                    pagos = await _reporteService.ObtenerPagosRecientesAsync(20); 
                }

                if (alerta.EnviarDeudasPendientes)
                {
                    deudas = await _reporteService.ObtenerListaDeudoresAsync();
                }

                if (alerta.EnviarProximosVencimientos)
                {
                    // En el futuro, idealmente un método para vencimientos próximos
                    vencimientos = new { Mensaje = "Próximos vencimientos activado." };
                }

                var resumen = new
                {
                    NuevosMiembros = miembros,
                    PagosRecientes = pagos,
                    Deudores = deudas,
                    ProximosVencimientos = vencimientos
                };

                await _webhookService.EnviarReporteProgramadoAsync(resumen, alerta.ChatIdDestino);
            }
        }
    }
}
