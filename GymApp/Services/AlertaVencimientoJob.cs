using GymApp.Data;
using GymApp.Models;
using GymApp.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public class AlertaVencimientoJob
    {
        private readonly GymDbContext _context;
        private readonly IWebhookService _webhookService;
        private readonly IConfiguracionAlertaRepository _configRepo;
        private readonly ILogger<AlertaVencimientoJob> _logger;

        public AlertaVencimientoJob(
            GymDbContext context, 
            IWebhookService webhookService, 
            IConfiguracionAlertaRepository configRepo,
            ILogger<AlertaVencimientoJob> logger)
        {
            _context = context;
            _webhookService = webhookService;
            _configRepo = configRepo;
            _logger = logger;
        }

        public async Task EjecutarAlertasAsync()
        {
            _logger.LogInformation($"[HANGFIRE] Iniciando revisión de vencimientos. Hora: {DateTime.Now:HH:mm:ss}");
            
            var configs = await _configRepo.GetAllAsync();
            var config = configs.FirstOrDefault();

            if (config == null)
            {
                _logger.LogWarning("[HANGFIRE] No hay configuración de alertas definida.");
                return;
            }

            if (!config.Activo)
            {
                _logger.LogInformation("[HANGFIRE] Configuración de alertas inactiva.");
                return;
            }

            if (config.UltimaEjecucionVencimientos?.Date == DateTime.Today)
            {
                _logger.LogInformation("[HANGFIRE] La revisión de vencimientos ya se realizó hoy.");
                return;
            }

            if (DateTime.Now.TimeOfDay < TimeSpan.FromHours(9))
            {
                _logger.LogInformation("[HANGFIRE] Aún no son las 9:00 AM para enviar alertas de vencimiento.");
                return;
            }

            var fechaObjetivo = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

            var membresias = await _context.Membresias
                .Include(m => m.User)
                .Include(m => m.Plan)
                .Where(m => m.FechaVencimiento == fechaObjetivo)
                .ToListAsync();

            _logger.LogInformation($"[HANGFIRE] Se encontraron {membresias.Count} membresías que vencen el {fechaObjetivo}");

            foreach (var m in membresias)
            {
                var payload = new 
                { 
                    Evento = "VENCIMIENTO_PROXIMO", 
                    ChatId = config.ChatIdDestino, 
                    Datos = new 
                    { 
                        Nombre = m.User.NombreCompleto, 
                        Plan = m.Plan.Nombre, 
                        VenceEl = m.FechaVencimiento.ToString("yyyy-MM-dd"), 
                        DiasRestantes = 7 
                    } 
                };

                _logger.LogInformation($"[HANGFIRE] Enviando recordatorio a {m.User.NombreCompleto} por vencimiento de {m.Plan.Nombre}");
                await _webhookService.EnviarAlertaInstantaneaAsync("VENCIMIENTO_PROXIMO", payload, config.ChatIdDestino);
            }

            config.UltimaEjecucionVencimientos = DateTime.Today;
            await _configRepo.UpdateAsync(config);
            await _configRepo.SaveAsync();
            _logger.LogInformation("[HANGFIRE] Ejecución de vencimientos finalizada correctamente.");
        }
    }
}
