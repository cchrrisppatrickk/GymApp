using System.Diagnostics;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using GymApp.Constants;
using Microsoft.AspNetCore.Authorization;
using GymApp.Services;

namespace GymApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IReporteService _reporteService;

        public HomeController(ILogger<HomeController> logger, IReporteService reporteService)
        {
            _logger = logger;
            _reporteService = reporteService;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = AppPoliticas.RequiereVerVentas)]
        public async Task<IActionResult> ObtenerDatosGrafico(string temporalidad = "Mes")
        {
            try {
                var datos = await _reporteService.ObtenerDatosGraficoTendenciaAsync(temporalidad);
                return Json(datos);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error obteniendo datos del gráfico");
                return BadRequest("Error interno al generar estadísticas.");
            }
        }

        /// <summary>
        /// Endpoint AJAX para el gráfico de tendencia de Pases Diarios,
        /// segmentado por turno (Mañana / Tarde).
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPoliticas.RequiereVerDashboard)]
        public async Task<IActionResult> ObtenerGraficoPasesDiarios(string temporalidad = "Mes")
        {
            try
            {
                var datos = await _reporteService.ObtenerGraficoPasesDiariosAsync(temporalidad);
                return Json(datos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo datos del gráfico de pases diarios");
                return BadRequest("Error interno al generar el gráfico de pases diarios.");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
