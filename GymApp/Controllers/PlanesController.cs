using GymApp.Models;
using GymApp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    public class PlanesController : Controller
    {
        private readonly IPlaneService _planeService;

        public PlanesController(IPlaneService planeService)
        {
            _planeService = planeService;
        }

        // Vista Principal (El contenedor de la SPA)
        public IActionResult Index()
        {
            return View();
        }

        // ==========================================
        //  API ENDPOINTS (JSON)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var planes = await _planeService.ObtenerTodosAsync();
            return Json(new { data = planes });
        }

        [HttpGet]
        public async Task<IActionResult> Obtener(int id)
        {
            var plan = await _planeService.ObtenerPorIdAsync(id);
            if (plan == null) return NotFound(new { success = false, message = "Plan no encontrado" });
            return Json(new { success = true, data = plan });
        }

        [HttpPost]
        public async Task<IActionResult> Guardar([FromBody] Plane plan)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Datos inválidos" });

            try
            {
                if (plan.PlanId == 0)
                    await _planeService.CrearPlanAsync(plan);
                else
                    await _planeService.ActualizarPlanAsync(plan);

                return Json(new { success = true, message = "Plan guardado correctamente" });
            }
            catch (ArgumentException ex) // Errores de negocio (precio negativo, etc)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _planeService.EliminarPlanAsync(id);
                return Json(new { success = true, message = "Plan eliminado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error al eliminar: " + ex.Message });
            }
        }
    }
}