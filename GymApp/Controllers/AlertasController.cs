using GymApp.Repositories;
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
            var configs = await _configuracionAlertaRepository.GetAllAsync();
            return View(configs);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ConfiguracionAlerta modelo, string[] diasSeleccionados)
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

                await _configuracionAlertaRepository.InsertAsync(modelo);
                await _configuracionAlertaRepository.SaveAsync();
                
                TempData["Success"] = "Configuración creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(modelo);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var config = await _configuracionAlertaRepository.GetByIdAsync(id);
            if (config == null) return NotFound();
            
            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ConfiguracionAlerta modelo, string[] diasSeleccionados)
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

                await _configuracionAlertaRepository.UpdateAsync(modelo);
                await _configuracionAlertaRepository.SaveAsync();
                
                TempData["Success"] = "Configuración actualizada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var config = await _configuracionAlertaRepository.GetByIdAsync(id);
            if (config != null)
            {
                await _configuracionAlertaRepository.DeleteAsync(id);
                await _configuracionAlertaRepository.SaveAsync();
                return Json(new { success = true, message = "Configuración eliminada correctamente." });
            }
            return Json(new { success = false, message = "Configuración no encontrada." });
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
