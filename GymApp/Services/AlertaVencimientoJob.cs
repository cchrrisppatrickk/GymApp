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

        public AlertaVencimientoJob(GymDbContext context, IWebhookService webhookService, IConfiguracionAlertaRepository configRepo)
        {
            _context = context;
            _webhookService = webhookService;
            _configRepo = configRepo;
        }

        public async Task EjecutarAlertasAsync()
        {
            var configs = await _configRepo.GetAllAsync();
            var config = configs.FirstOrDefault();

            if (config == null || !config.Activo) return;

            if (config.UltimaEjecucionVencimientos?.Date == DateTime.Today) return;

            if (DateTime.Now.TimeOfDay < TimeSpan.FromHours(9)) return;

            var fechaObjetivo = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

            var membresias = await _context.Membresias
                .Include(m => m.User)
                .Include(m => m.Plan)
                .Where(m => m.FechaVencimiento == fechaObjetivo)
                .ToListAsync();

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

                await _webhookService.EnviarAlertaInstantaneaAsync("VENCIMIENTO_PROXIMO", payload, config.ChatIdDestino);
            }

            config.UltimaEjecucionVencimientos = DateTime.Today;
            await _configRepo.UpdateAsync(config);
            await _configRepo.SaveAsync();
        }
    }
}
