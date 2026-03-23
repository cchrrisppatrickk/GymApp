using GymApp.Services;
using GymApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    [Authorize(Roles = "Admin,Empleado")]
    public class CongelamientosController : BaseController
    {
        private readonly ICongelamientoService _congelamientoService;

        public CongelamientosController(ICongelamientoService congelamientoService)
        {
            _congelamientoService = congelamientoService;
        }

        // Obtiene el historial de congelamientos de una membresía en formato JSON
        [HttpGet]
        public async Task<IActionResult> Historial(int id)
        {
            try
            {
                var historial = await _congelamientoService.ListarHistorialAsync(id);
                // Mapeo simple para evitar problemas de referencia circular en el JSON
                var data = historial.Select(c => new {
                    id = c.CongelamientoId,
                    inicio = c.FechaInicio.ToString("dd/MM/yyyy"),
                    fin = c.FechaFin.ToString("dd/MM/yyyy"),
                    motivo = c.Motivo,
                    empleado = c.UsuarioEmpleado?.NombreCompleto,
                    registro = c.FechaRegistro?.ToString("dd/MM/yyyy HH:mm")
                });
                
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Acción para congelar una membresía
        [HttpPost]
        public async Task<IActionResult> Congelar([FromBody] CongelarMembresiaDTO model)
        {
            try
            {
                // Inyectamos el ID del empleado que está logueado actualmente
                model.UsuarioEmpleadoId = this.CurrentUserId;
                
                await _congelamientoService.CongelarAsync(model);
                
                return Json(new { success = true, message = "La membresía ha sido congelada correctamente y su fecha de vencimiento ha sido extendida." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Acción para descongelar manualmente (si el cliente regresa antes)
        [HttpPost]
        public async Task<IActionResult> Descongelar(int id)
        {
            try
            {
                await _congelamientoService.DescongelarManualAsync(id);
                return Json(new { success = true, message = "La membresía ha sido reactivada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
