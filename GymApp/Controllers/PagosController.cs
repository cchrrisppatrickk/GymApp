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

        public async Task<IActionResult> Index(string? buscar, int? mes, int? anio, int pagina = 1)
        {
            if (!mes.HasValue || !anio.HasValue)
            {
                mes = DateTime.Now.Month;
                anio = DateTime.Now.Year;
            }

            var result = await _pagoService.ObtenerPagosPaginadosAsync(buscar, mes, anio, pagina);

            ViewBag.Buscar = buscar;
            ViewBag.Mes = mes;
            ViewBag.Anio = anio;

            return View(result);
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
        public async Task<IActionResult> BuscarDeuda(string termino)
        {
            try
            {
                var resultados = await _pagoService.BuscarDeudaClienteAsync(termino);
                return Json(new { success = true, data = resultados });
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