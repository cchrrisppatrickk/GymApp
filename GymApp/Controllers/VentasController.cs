using GymApp.Services;
using GymApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    [Authorize(Roles = "Admin,Empleado")]
    public class VentasController : BaseController
    {
        private readonly IVentaService _ventaService;

        public VentasController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        public IActionResult Index()
        {
            return View(); // Aquí irá la pantalla POS (Quiosco)
        }

        [HttpGet]
        public async Task<IActionResult> Historial()
        {
            // Podrías devolver una vista diferente para ver reportes
            var historial = await _ventaService.HistorialVentasAsync();
            // Mapear a un DTO visual si es necesario, o devolver JSON
            return Json(new { data = historial });
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] VentaCreateDTO modelo)
        {
            try
            {
                // Obtener ID del empleado logueado
                var empleadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int empleadoId = int.Parse(empleadoIdClaim ?? "0");

                if (modelo.Items == null || modelo.Items.Count == 0)
                {
                    return BadRequest(new { success = false, message = "El carrito está vacío" });
                }

                int ventaId = await _ventaService.RegistrarVentaAsync(modelo, empleadoId);

                return Json(new { success = true, message = "Venta registrada correctamente", ventaId = ventaId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}