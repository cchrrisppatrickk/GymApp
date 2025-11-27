using GymApp.Models;
using GymApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    [Authorize] // Solo personal autorizado gestiona productos
    public class ProductosController : Controller
    {
        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        // VISTA PRINCIPAL
        public IActionResult Index()
        {
            return View();
        }

        // ================= API JSON =================

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var lista = await _productoService.ListarProductosAsync();
            return Json(new { data = lista });
        }

        [HttpGet]
        public async Task<IActionResult> Obtener(int id)
        {
            var prod = await _productoService.ObtenerPorIdAsync(id);
            if (prod == null) return NotFound(new { success = false, message = "Producto no existe" });
            return Json(new { success = true, data = prod });
        }

        [HttpPost]
        public async Task<IActionResult> Guardar([FromBody] Producto model)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Datos inválidos" });

            try
            {
                if (model.ProductoId == 0)
                    await _productoService.CrearProductoAsync(model);
                else
                    await _productoService.ActualizarProductoAsync(model);

                return Json(new { success = true, message = "Producto guardado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _productoService.EliminarProductoAsync(id);
                return Json(new { success = true, message = "Producto eliminado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}