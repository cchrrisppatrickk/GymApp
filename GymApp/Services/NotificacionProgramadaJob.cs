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

                var hoy = DateTime.Today;
                var hace7Dias = hoy.AddDays(-7);
                var en7Dias = hoy.AddDays(7);
                
                // Para campos DateOnly (Membresia)
                var hoyDate = DateOnly.FromDateTime(hoy);
                var en7DiasDate = DateOnly.FromDateTime(en7Dias);

                object nuevosMiembros = null;
                object pagosRecientes = null;
                object proximosVencimientos = null;
                object deudas = null;
                object finanzas = null;

                // 1. Pagos Estrictos (Últimos 7 días) - Excluyendo Anulados
                if (alerta.EnviarPagosHoy)
                {
                    pagosRecientes = await _context.PagosMembresia
                        .Include(p => p.Membresia).ThenInclude(m => m.User)
                        .Include(p => p.Membresia).ThenInclude(m => m.Plan)
                        .Where(p => p.FechaPago.HasValue && p.FechaPago.Value.Date >= hace7Dias && p.FechaPago.Value.Date <= hoy && !p.EsAnulado)
                        .Select(p => new {
                            NombreCliente = p.Membresia.User.NombreCompleto,
                            NombrePlan = p.Membresia.Plan.Nombre,
                            Monto = p.Monto,
                            FechaPago = p.FechaPago,
                            MetodoPago = p.MetodoPago
                        })
                        .OrderByDescending(p => p.FechaPago)
                        .ToListAsync();
                }

                // 2. Nuevos Miembros Estrictos (Últimos 7 días)
                if (alerta.EnviarNuevosMiembros)
                {
                    nuevosMiembros = await _context.Usuarios
                        .Where(u => u.FechaRegistro.HasValue && u.FechaRegistro.Value.Date >= hace7Dias && u.FechaRegistro.Value.Date <= hoy)
                        .Select(u => new {
                            u.UserId,
                            u.NombreCompleto,
                            u.Dni,
                            u.Telefono,
                            u.FechaRegistro
                        })
                        .OrderByDescending(u => u.FechaRegistro)
                        .ToListAsync();
                }

                // 3. Vencimientos Reales (Entre hoy y 7 días)
                if (alerta.EnviarProximosVencimientos)
                {
                    // A. Filtramos primero en la base de datos y lo traemos a memoria
                    var vencimientosDb = await _context.Membresias
                        .Include(m => m.User)
                        .Include(m => m.Plan)
                        .Where(m => m.Estado == "Activa" && m.FechaVencimiento >= hoyDate && m.FechaVencimiento <= en7DiasDate)
                        .OrderBy(m => m.FechaVencimiento)
                        .ToListAsync();

                    // B. Calculamos los días restantes en memoria (C#) para evitar errores de traducción de EF Core
                    proximosVencimientos = vencimientosDb.Select(m => new {
                        NombreCliente = m.User.NombreCompleto,
                        NombrePlan = m.Plan.Nombre,
                        FechaVencimiento = m.FechaVencimiento,
                        DiasRestantes = (m.FechaVencimiento.ToDateTime(TimeOnly.MinValue) - hoy).Days
                    }).ToList();
                }

                // 4. Deudas (Usando el servicio existente por ahora, ya que es complejo)
                if (alerta.EnviarDeudasPendientes)
                    deudas = await _reporteService.ObtenerListaDeudoresAsync();

                // 5. Resumen Financiero Estricto (Sumatoria 7 días)
                if (alerta.EnviarResumenFinanciero)
                {
                    var todosPagos7Dias = await _context.PagosMembresia
                        .Where(p => p.FechaPago.HasValue && p.FechaPago.Value.Date >= hace7Dias && p.FechaPago.Value.Date <= hoy && !p.EsAnulado)
                        .ToListAsync();

                    finanzas = new {
                        TotalIngresos = todosPagos7Dias.Sum(p => p.Monto),
                        IngresosPorMetodo = todosPagos7Dias.GroupBy(p => p.MetodoPago)
                            .Select(g => new { Metodo = g.Key, Total = g.Sum(p => p.Monto) }),
                        CantidadTransacciones = todosPagos7Dias.Count,
                        PromedioPorPago = todosPagos7Dias.Any() ? todosPagos7Dias.Average(p => p.Monto) : 0
                    };
                }

                var resumen = new
                {
                    Titulo = "Mega Reporte Programado (Últimos 7 Días)",
                    FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    Periodo = $"{hace7Dias:dd/MM/yyyy} al {hoy:dd/MM/yyyy}",
                    NuevosMiembros = nuevosMiembros,
                    PagosRecientes = pagosRecientes,
                    Deudores = deudas,
                    Finanzas = finanzas,
                    ProximosVencimientos = proximosVencimientos
                };

                bool exito = await _webhookService.EnviarReporteProgramadoAsync(resumen, alerta.ChatIdDestino);

                if (exito)
                {
                    alerta.UltimaEjecucionReporte = DateTime.Now;
                    _context.Entry(alerta).Property(x => x.UltimaEjecucionReporte).IsModified = true;
                    _logger.LogInformation($"[HANGFIRE] Alerta {alerta.Id}: Reporte enviado con éxito a n8n.");
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
