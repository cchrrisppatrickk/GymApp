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

        public AlertasController(IConfiguracionAlertaRepository configuracionAlertaRepository, IWebhookService webhookService)
        {
            _configuracionAlertaRepository = configuracionAlertaRepository;
            _webhookService = webhookService;
        }

        public async Task<IActionResult> Index()
        {
            var config = await _configuracionAlertaRepository.ObtenerConfiguracionGlobalAsync();
            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(ConfiguracionAlerta modelo, string[] diasSeleccionados)
        {
            if (ModelState.IsValid)
            {
                if (diasSeleccionados != null && diasSeleccionados.Any())
                {
                    modelo.DiasSemana = string.Join(",", diasSeleccionados);
                }
                else
                {
                    modelo.DiasSemana = string.Empty;
                }

                var exito = await _configuracionAlertaRepository.GuardarConfiguracionGlobalAsync(modelo);
                
                if (exito)
                {
                    TempData["Success"] = "Configuración global actualizada exitosamente.";
                }
                else
                {
                    TempData["Error"] = "No se pudo guardar la configuración.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            return View("Index", modelo);
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
    }
}
