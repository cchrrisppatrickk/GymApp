using GymApp.Repositories;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AlertasController : BaseController
    {
        private readonly IConfiguracionAlertaRepository _configuracionAlertaRepository;

        public AlertasController(IConfiguracionAlertaRepository configuracionAlertaRepository)
        {
            _configuracionAlertaRepository = configuracionAlertaRepository;
        }

        public async Task<IActionResult> Index()
        {
            var configs = await _configuracionAlertaRepository.GetAllAsync();
            var config = configs.FirstOrDefault();
            
            if (config == null)
            {
                config = new ConfiguracionAlerta
                {
                    HoraEnvio = new TimeSpan(8, 0, 0),
                    Activo = true
                };
            }
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

                if (modelo.Id == 0)
                {
                    await _configuracionAlertaRepository.InsertAsync(modelo);
                }
                else
                {
                    await _configuracionAlertaRepository.UpdateAsync(modelo);
                }
                await _configuracionAlertaRepository.SaveAsync();
                
                TempData["Success"] = "Configuración de alertas guardada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            
            TempData["Error"] = "Hubo un error al guardar la configuración.";
            return View("Index", modelo);
        }
    }
}
