using System;
using System.Linq;
using System.Threading.Tasks;
using GymApp.Repositories;
using GymApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services
{
    public class NotificacionProgramadaJob
    {
        private readonly IConfiguracionAlertaRepository _configRepo;
        private readonly IReporteService _reporteService;
        private readonly IUsuarioService _usuarioService;
        private readonly IWebhookService _webhookService;
        private readonly GymDbContext _context;

        public NotificacionProgramadaJob(
            IConfiguracionAlertaRepository configRepo,
            IReporteService reporteService,
            IUsuarioService usuarioService,
            IWebhookService webhookService,
            GymDbContext context)
        {
            _configRepo = configRepo;
            _reporteService = reporteService;
            _usuarioService = usuarioService;
            _webhookService = webhookService;
            _context = context;
        }

        public async Task EjecutarRevisionAsync()
        {
            var hoy = DateTime.Today;
            var horaActual = DateTime.Now.TimeOfDay;
            var diaActual = ((int)DateTime.Now.DayOfWeek).ToString();

            var alertasCandidatas = await _configRepo.ObtenerAlertasParaEjecutarAsync(horaActual, diaActual);

            foreach (var alerta in alertasCandidatas)
            {
                // Idempotencia: No ejecutar si ya se envió hoy
                if (alerta.UltimaEjecucionReporte.HasValue && alerta.UltimaEjecucionReporte.Value.Date == hoy)
                    continue;

                object miembros = null;
                object pagos = null;
                object deudas = null;
                object vencimientos = null;
                object finanzas = null;

                if (alerta.EnviarNuevosMiembros)
                    miembros = await _usuarioService.ObtenerRecientesParaAgenteAsync(1);

                if (alerta.EnviarPagosHoy)
                    pagos = await _reporteService.ObtenerPagosRecientesAsync(10);

                if (alerta.EnviarDeudasPendientes)
                    deudas = await _reporteService.ObtenerListaDeudoresAsync();

                if (alerta.EnviarResumenFinanciero)
                    finanzas = await _reporteService.ObtenerEstadisticasFinancierasAsync();

                if (alerta.EnviarProximosVencimientos)
                    vencimientos = new { Mensaje = "Revisar panel para detalles de vencimientos." };

                var resumen = new
                {
                    Titulo = "Mega Reporte Programado",
                    Fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    NuevosMiembros = miembros,
                    PagosRecientes = pagos,
                    Deudores = deudas,
                    Finanzas = finanzas,
                    ProximosVencimientos = vencimientos
                };

                bool exito = await _webhookService.EnviarReporteProgramadoAsync(resumen, alerta.ChatIdDestino);

                if (exito)
                {
                    alerta.UltimaEjecucionReporte = DateTime.Now;
                    _context.Entry(alerta).Property(x => x.UltimaEjecucionReporte).IsModified = true;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
