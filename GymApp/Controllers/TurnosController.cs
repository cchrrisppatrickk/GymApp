using GymApp.Models;
using GymApp.Services;
using GymApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    public class TurnosController : Controller
    {
        private readonly ITurnoService _turnoService;

        public TurnosController(ITurnoService turnoService)
        {
            _turnoService = turnoService;
        }

        // 1. VISTA PRINCIPAL (La única que devuelve HTML)
        public IActionResult Index()
        {
            return View();
        }

        // ==========================================
        //  API ENDPOINTS (Devuelven JSON para JS)
        // ==========================================

        // GET: Obtener lista de turnos
        [HttpGet]
        public async Task<IActionResult> ListarTurnos()
        {
            var turnos = await _turnoService.ObtenerTodosAsync();
            // Retornamos un JSON puro
            return Json(new { data = turnos });
        }

        // GET: Obtener un turno para editar (llenar el modal)
        [HttpGet]
        public async Task<IActionResult> ObtenerTurno(int id)
        {
            var turno = await _turnoService.ObtenerPorIdAsync(id);
            if (turno == null) return NotFound(new { success = false, message = "Turno no encontrado" });

            return Json(new { success = true, data = turno });
        }

        // POST: Guardar (Sirve para Crear Y Editar)
        [HttpPost]
        public async Task<IActionResult> Guardar([FromBody] TurnoDTO modelo)
        {
            // 1. DETECCIÓN DE ERRORES DE VALIDACIÓN
            if (!ModelState.IsValid)
            {
                // Esto recopila los errores específicos y te los devuelve al frontend
                var errores = string.Join("; ", ModelState.Values
                                                .SelectMany(x => x.Errors)
                                                .Select(x => x.ErrorMessage));
                return BadRequest(new { success = false, message = "Datos inválidos: " + errores });
            }

            try
            {
                // 2. MAPEO: Convertir de DTO (String) a Entidad (TimeSpan)
                var turnoEntidad = new Turno
                {
                    TurnoId = modelo.TurnoId,
                    Nombre = modelo.Nombre,
                    Descripcion = modelo.Descripcion,
                    // Convertimos el string "HH:mm" a TimeSpan
                    HoraInicio = TimeSpan.Parse(modelo.HoraInicio),
                    HoraFin = TimeSpan.Parse(modelo.HoraFin)
                };

                if (modelo.TurnoId == 0)
                {
                    await _turnoService.CrearTurnoAsync(turnoEntidad);
                }
                else
                {
                    await _turnoService.ActualizarTurnoAsync(turnoEntidad);
                }

                return Json(new { success = true, message = "Guardado correctamente" });
            }
            catch (ArgumentException ex) // Errores de lógica (ej: Fin < Inicio)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex) // Errores técnicos
            {
                return StatusCode(500, new { success = false, message = "Error interno: " + ex.Message });
            }
        }

        // POST: Eliminar
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _turnoService.EliminarTurnoAsync(id);
                return Json(new { success = true, message = "Turno eliminado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "No se pudo eliminar: " + ex.Message });
            }
        }
    }
}