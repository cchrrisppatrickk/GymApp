using GymApp.Services;
using GymApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    [Authorize(Roles = "Admin,Empleado")]
    public class MembresiasController : BaseController
    {
        private readonly IMembresiaService _membresiaService;

        // Inyectamos también los servicios de planes y turnos para llenar los combos
        private readonly IPlaneService _planeService;
        private readonly ITurnoService _turnoService;

        public MembresiasController(IMembresiaService mService, IPlaneService pService, ITurnoService tService)
        {
            _membresiaService = mService;
            _planeService = pService;
            _turnoService = tService;
        }

        [HttpGet]
        public IActionResult Index(int? userId)
        {
            ViewBag.PreselectUserId = userId;
            return View();
        }

        [HttpGet]
        public IActionResult Crear(int? userId)
        {
            return RedirectToAction("Index", new { userId = userId });
        }


        // API: Listar con filtro
        [HttpGet]
        public async Task<IActionResult> Listar(string filtro = "todas")
        {
            var data = await _membresiaService.ListarMembresiasAsync(filtro);
            return Json(new { data });
        }

        // API: Datos para llenar el Modal (Planes y Turnos)
        [HttpGet]
        public async Task<IActionResult> ObtenerRecursosCreacion()
        {
            var planes = await _planeService.ObtenerTodosAsync();
            var turnos = await _turnoService.ObtenerTodosAsync();
            return Json(new { planes, turnos });
        }

        // API: Autocomplete de Usuarios
        [HttpGet]
        public async Task<IActionResult> BuscarUsuario(string q)
        {
            var resultados = await _membresiaService.BuscarClientesAsync(q ?? "");
            return Json(resultados);
        }

        // API: Crear
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] MembresiaCreateDTO model)
        {
            try
            {
                var id = await _membresiaService.CrearMembresiaAsync(model);
                return Json(new { success = true, message = "Membresía registrada correctamente", id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await _membresiaService.ObtenerDetallesAsync(id);
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Congelar(int membresiaId, DateTime fechaFin, string motivo)
        {
            try
            {
                var dateFin = DateOnly.FromDateTime(fechaFin);
                await _membresiaService.CongelarMembresiaAsync(membresiaId, CurrentUserId, dateFin, motivo);
                TempData["Success"] = "Membresía congelada con éxito. La vigencia ha sido extendida.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", new { id = membresiaId });
        }

        [HttpGet]
        public async Task<IActionResult> GetPropuestaRenovacion(int id)
        {
            var fecha = await _membresiaService.ObtenerPropuestaRenovacionAsync(id);
            return Json(new { fechaInicio = fecha.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        public async Task<IActionResult> Renovar(int userId, int planId, int turnoId, DateTime fechaInicio)
        {
            try
            {
                var dto = new MembresiaCreateDTO
                {
                    UserId = userId,
                    PlanId = planId,
                    TurnoId = turnoId,
                    FechaInicio = fechaInicio
                };

                var nuevaMembresiaId = await _membresiaService.CrearMembresiaAsync(dto);

                TempData["Success"] = "Membresía renovada con éxito. <a href='/Pagos/Index' class='fw-bold text-decoration-underline'>Ir a Caja para registrar cobro</a>";
                return RedirectToAction("Details", new { id = nuevaMembresiaId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}