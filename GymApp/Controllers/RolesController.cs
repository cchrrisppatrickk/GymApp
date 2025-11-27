using GymApp.Models;
using GymApp.Services;
using GymApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers
{
    [Authorize(Roles = "Admin,Empleado")]
    public class RolesController : BaseController
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: Roles (Vista Principal)
        public async Task<IActionResult> Index()
        {
            var rolesEntities = await _roleService.ObtenerTodosAsync();
            var rolesViewModels = rolesEntities.Select(r => new RoleViewModel
            {
                RoleId = r.RoleId,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion
            }).ToList();

            return View(rolesViewModels);
        }

        // GET: Obtener un solo rol (Para llenar el Modal de Editar)
        [HttpGet]
        public async Task<IActionResult> GetRole(int id)
        {
            var role = await _roleService.ObtenerPorIdAsync(id);
            if (role == null) return NotFound();

            return Json(new { success = true, data = role });
        }

        // POST: Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Datos inválidos", errors = errors });
            }

            try
            {
                var roleEntity = new Role
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion
                };

                await _roleService.CrearRolAsync(roleEntity);
                return Json(new { success = true, message = "Rol creado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] RoleViewModel model)
        {
            if (!ModelState.IsValid) return Json(new { success = false, message = "Datos inválidos" });

            try
            {
                var roleEntity = new Role
                {
                    // CORRECCIÓN DEL ERROR:
                    // Convertimos explícitamente el 'int?' del ViewModel al 'int' de la Entidad.
                    // (model.RoleId ?? 0) significa: "Si es null, usa 0".
                    RoleId = (int)(model.RoleId ?? 0),
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion
                };

                await _roleService.ActualizarRolAsync(roleEntity);
                return Json(new { success = true, message = "Rol actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Eliminar
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var resultado = await _roleService.EliminarRolAsync(id);
                if (!resultado) return Json(new { success = false, message = "Rol no encontrado" });

                return Json(new { success = true, message = "Rol eliminado correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                // Aquí capturamos tu regla de negocio (No borrar Admin/Portero)
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Ocurrió un error inesperado al eliminar." });
            }
        }
    }
}