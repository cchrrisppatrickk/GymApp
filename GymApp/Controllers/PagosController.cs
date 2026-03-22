using GymApp.Services;
using GymApp.ViewModels;
using Microsoft.AspNetCore.Authorization; // Necesario para identificar quién cobra
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims; // Para leer los Claims del usuario
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    [Authorize(Roles = "Admin,Empleado")]
    public class PagosController : BaseController
    {
        private readonly IPagoService _pagoService;

        public PagosController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // VISTA DE IMPRESIÓN (Simple HTML para imprimir)
        public IActionResult Comprobante(int id)
        {
            // Aquí deberías buscar el pago por ID y retornarlo a una vista limpia
            // return View(pago);
            return Content($"Vista de impresión del comprobante #{id}");
        }

        // API: Listar Historial
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var data = await _pagoService.ListarPagosAsync();
            return Json(new { data });
        }

        // API: Buscar Membresía al escribir DNI
        [HttpGet]
        public async Task<IActionResult> BuscarDeuda(string dni)
        {
            try
            {
                var info = await _pagoService.BuscarMembresiaPorDniAsync(dni);
                return Json(new { success = true, data = info });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Registrar Cobro
        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] PagoCreateDTO model)
        {
            try
            {
                // AUDITORÍA: Obtenemos el ID del empleado logueado desde la Cookie/Token
                // Asumiendo que guardaste el UserID en los Claims al hacer Login
                var empleadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int empleadoId = int.Parse(empleadoIdClaim ?? "1"); // Fallback a 1 si es prueba

                int pagoId = await _pagoService.RegistrarPagoAsync(model, empleadoId);

                return Json(new { success = true, message = "Pago registrado", pagoId = pagoId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


    }
}