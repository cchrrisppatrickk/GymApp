using GymApp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    public class ReportesController : Controller
    {
        private readonly IReporteService _reporteService;

        public ReportesController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        public IActionResult Index()
        {
            // Por defecto muestra el mes actual
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerIngresos(int mes, int anio)
        {
            var data = await _reporteService.ObtenerReporteMensualAsync(mes, anio);
            return Json(new { success = true, data = data });
        }
    }
}