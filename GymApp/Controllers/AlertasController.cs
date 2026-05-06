using GymApp.Repositories;
using GymApp.Services;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AlertasController : BaseController
    {
        private readonly IConfiguracionAlertaRepository _configuracionAlertaRepository;
        private readonly IWebhookService _webhookService;
        private readonly NotificacionProgramadaJob _programadaJob;

        public AlertasController(
            IConfiguracionAlertaRepository configuracionAlertaRepository, 
            IWebhookService webhookService,
            NotificacionProgramadaJob programadaJob)
        {
            _configuracionAlertaRepository = configuracionAlertaRepository;
            _webhookService = webhookService;
            _programadaJob = programadaJob;
        }

        public async Task<IActionResult> Index()
        {
            var configs = await _configuracionAlertaRepository.GetAllAsync();
            return View(configs);
        }

        public IActionResult Crear()
        {
            var model = new ConfiguracionAlerta
            {
                Activo = true,
                HoraEnvio = new System.TimeSpan(20, 0, 0)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ConfiguracionAlerta modelo, string[] diasSeleccionados)
        {
            if (ModelState.IsValid)
            {
                modelo.DiasSemana = diasSeleccionados != null ? string.Join(",", diasSeleccionados) : string.Empty;

                await _configuracionAlertaRepository.InsertAsync(modelo);
                await _configuracionAlertaRepository.SaveAsync();

                TempData["Success"] = "Configuración creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(modelo);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var model = await _configuracionAlertaRepository.GetByIdAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ConfiguracionAlerta modelo, string[] diasSeleccionados)
        {
            if (ModelState.IsValid)
            {
                modelo.DiasSemana = diasSeleccionados != null ? string.Join(",", diasSeleccionados) : string.Empty;

                await _configuracionAlertaRepository.UpdateAsync(modelo);
                await _configuracionAlertaRepository.SaveAsync();

                TempData["Success"] = "Configuración actualizada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _configuracionAlertaRepository.DeleteAsync(id);
            await _configuracionAlertaRepository.SaveAsync();
            TempData["Success"] = "Configuración eliminada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ProbarConexion([FromBody] string chatId)
        {
            if (string.IsNullOrWhiteSpace(chatId))
            {
                return Json(new { success = false, message = "El ID de chat no puede estar vacío." });
            }

            var result = await _webhookService.EnviarMensajePruebaAsync(chatId);
            if (result)
            {
                return Json(new { success = true, message = "Mensaje enviado, revisa tu Telegram." });
            }

            return Json(new { success = false, message = "Error de conexión. Revisa n8n y tu Webhook." });
        }

        [HttpPost]
        public async Task<IActionResult> EjecutarReporteManual(int id)
        {
            // Ejecutamos la revisión completa. 
            // Nota: Esto respetará las validaciones de "Ya se ejecutó hoy" o "No es el día".
            // Para una prueba REAL, el usuario debe asegurarse de que la config sea válida para hoy/ahora
            // o podemos agregar un parámetro al job para "saltar validaciones".
            await _programadaJob.EjecutarRevisionAsync();
            
            return Json(new { success = true, message = "Proceso de revisión de jobs iniciado. Revisa los logs o Telegram." });
        }
    }
}
