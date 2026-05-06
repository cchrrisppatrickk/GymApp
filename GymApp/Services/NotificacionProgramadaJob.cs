using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymApp.Repositories;
using GymApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymApp.Services
{
    public class NotificacionProgramadaJob
    {
        private readonly IConfiguracionAlertaRepository _configRepo;
        private readonly IReporteService _reporteService;
        private readonly IUsuarioService _usuarioService;
        private readonly IWebhookService _webhookService;
        private readonly GymDbContext _context;
        private readonly ILogger<NotificacionProgramadaJob> _logger;

        public NotificacionProgramadaJob(
            IConfiguracionAlertaRepository configRepo,
            IReporteService reporteService,
            IUsuarioService usuarioService,
            IWebhookService webhookService,
            GymDbContext context,
            ILogger<NotificacionProgramadaJob> logger)
        {
            _configRepo = configRepo;
            _reporteService = reporteService;
            _usuarioService = usuarioService;
            _webhookService = webhookService;
            _context = context;
            _logger = logger;
        }

        public async Task EjecutarRevisionAsync()
        {
            var horaActual = DateTime.Now;
            _logger.LogInformation($"[HANGFIRE] Iniciando Job. Hora del Servidor: {horaActual:HH:mm:ss}, Día: {horaActual.DayOfWeek}");

            // Obtenemos todas para poder loguear por qué algunas no se ejecutan
            var todasConfigs = await _configRepo.GetAllAsync();
            
            if (todasConfigs == null || !todasConfigs.Any())
            {
                _logger.LogWarning("[HANGFIRE] No se encontraron configuraciones de alertas. Abortando.");
                return;
            }

            foreach (var alerta in todasConfigs)
            {
                if (!alerta.Activo)
                {
                    _logger.LogWarning($"[HANGFIRE] Configuración ID {alerta.Id} inactiva. Saltando.");
                    continue;
                }

                // Mapeo de Días
                string diaEspanol = TraducirDiaAlEspanol(horaActual.DayOfWeek);
                if (string.IsNullOrEmpty(alerta.DiasSemana) || !alerta.DiasSemana.Contains(diaEspanol))
                {
                    _logger.LogInformation($"[HANGFIRE] Alerta {alerta.Id}: Hoy es {diaEspanol} y no está en la configuración ({alerta.DiasSemana}). Saltando.");
                    continue;
                }

                // Validación de Hora
                if (horaActual.TimeOfDay < alerta.HoraEnvio)
                {
                    _logger.LogInformation($"[HANGFIRE] Alerta {alerta.Id}: Aún no es la hora. Actual: {horaActual.TimeOfDay:hh\\:mm}, Configurada: {alerta.HoraEnvio:hh\\:mm}. Saltando.");
                    continue;
                }

                // Validación Idempotencia
                if (alerta.UltimaEjecucionReporte.HasValue && alerta.UltimaEjecucionReporte.Value.Date == horaActual.Date)
                {
                    _logger.LogInformation($"[HANGFIRE] Alerta {alerta.Id}: El reporte ya se envió hoy. Saltando.");
                    continue;
                }

                _logger.LogInformation($"[HANGFIRE] Alerta {alerta.Id}: Construyendo payload y enviando a n8n...");

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
                    _logger.LogInformation($"[HANGFIRE] Alerta {alerta.Id}: Reporte enviado con éxito. UltimaEjecucion actualizada a {alerta.UltimaEjecucionReporte}");
                }
                else
                {
                    _logger.LogError($"[HANGFIRE] Alerta {alerta.Id}: Error al enviar el reporte a n8n.");
                }
            }

            await _context.SaveChangesAsync();
        }

        private string TraducirDiaAlEspanol(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miércoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sábado",
                DayOfWeek.Sunday => "Domingo",
                _ => day.ToString()
            };
        }
    }
}
