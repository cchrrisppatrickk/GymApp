using GymApp.Services;
using GymApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    public class MembresiasController : Controller
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
        public IActionResult Index()
        {
            return View();
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
                await _membresiaService.CrearMembresiaAsync(model);
                return Json(new { success = true, message = "Membresía registrada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}