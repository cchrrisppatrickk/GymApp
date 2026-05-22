using GymApp.Constants;
using GymApp.Data;
using GymApp.Models;
using GymApp.Repositories;
using GymApp.Services;
using GymApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers
{
    /// <summary>
    /// Panel de Administración exclusivo para el rol Admin.
    /// Gestiona el personal (Empleados/Admins) y su matriz de permisos.
    /// Los socios (Clientes) se gestionan en UsuariosController.
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    public class AdministracionController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IRoleRepository _rolesRepository;
        private readonly GymDbContext _context;

        public AdministracionController(
            IUsuarioService usuarioService,
            IRoleRepository rolesRepository,
            GymDbContext context)
        {
            _usuarioService = usuarioService;
            _rolesRepository = rolesRepository;
            _context = context;
        }

        // ============================================================
        // INDEX — Lista del Personal (Admin + Empleados)
        // ============================================================
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Gestión de Personal";
            var personal = await _usuarioService.ObtenerPersonalAsync();
            await CargarViewBags(); // ← necesario para que el modal tenga roles y permisos
            return View(personal);
        }

        // ============================================================
        // GET: Formulario Crear Personal
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> CrearPersonal()
        {
            ViewData["Title"] = "Nuevo Empleado";
            await CargarViewBags();
            return View("FormularioPersonal", new UsuarioViewModel());
        }

        // ============================================================
        // GET: Datos del empleado en JSON (para AJAX del modal)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> EditarPersonal(int id)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);
            if (usuario == null) return NotFound();

            var permisos = await _usuarioService.ObtenerPermisosUsuarioAsync(id);

            // Si es petición AJAX desde el modal devolvemos JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    userId = usuario.UserId,
                    roleId = usuario.RoleId,
                    nombreCompleto = usuario.NombreCompleto,
                    nombreUsuario = usuario.NombreUsuario,
                    dni = usuario.Dni,
                    email = usuario.Email,
                    telefono = usuario.Telefono,
                    estado = usuario.Estado ?? true,
                    permisos
                });
            }

            // Fallback para acceso directo (sin modal)
            ViewData["Title"] = $"Editar — {usuario.NombreCompleto}";
            var vm = new UsuarioViewModel
            {
                UserId = usuario.UserId,
                RoleId = usuario.RoleId,
                NombreCompleto = usuario.NombreCompleto,
                NombreUsuario = usuario.NombreUsuario,
                Dni = usuario.Dni,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Estado = usuario.Estado ?? true,
                PermisosSeleccionados = permisos.ToArray()
            };

            await CargarViewBags();
            return View("FormularioPersonal", vm);
        }

        // ============================================================
        // POST: Guardar (Crear o Editar)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPersonal([FromForm] UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Datos incompletos.", errors });
            }

            try
            {
                if (model.UserId == 0)
                {
                    // --- CREAR NUEVO EMPLEADO ---
                    var nuevo = new Usuario
                    {
                        RoleId = model.RoleId,
                        NombreCompleto = model.NombreCompleto,
                        NombreUsuario = model.NombreUsuario,
                        Dni = model.Dni,
                        Email = model.Email,
                        Telefono = model.Telefono,
                        Estado = true
                    };

                    var creado = await _usuarioService.CrearUsuarioAsync(nuevo, model.Password, model.FotoArchivo, model.FotoBase64);
                    await _usuarioService.ActualizarPermisosUsuarioAsync(creado.UserId, model.PermisosSeleccionados ?? Array.Empty<string>());

                    return Json(new { success = true, message = $"Empleado '{creado.NombreCompleto}' creado exitosamente." });
                }
                else
                {
                    // --- EDITAR EMPLEADO EXISTENTE ---
                    var existente = await _usuarioService.ObtenerPorIdAsync(model.UserId);
                    if (existente == null) return Json(new { success = false, message = "Empleado no encontrado." });

                    existente.RoleId = model.RoleId;
                    existente.NombreCompleto = model.NombreCompleto;
                    existente.Dni = model.Dni;
                    existente.Email = model.Email;
                    existente.Telefono = model.Telefono;
                    existente.Estado = model.Estado;

                    if (!string.IsNullOrEmpty(model.NombreUsuario))
                        existente.NombreUsuario = model.NombreUsuario;

                    if (!string.IsNullOrEmpty(model.Password))
                        existente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

                    await _usuarioService.ActualizarUsuarioAsync(existente, model.FotoArchivo, model.FotoBase64);
                    await _usuarioService.ActualizarPermisosUsuarioAsync(existente.UserId, model.PermisosSeleccionados ?? Array.Empty<string>());

                    return Json(new { success = true, message = $"'{existente.NombreCompleto}' actualizado correctamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // POST: Eliminar Personal
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> EliminarPersonal(int id)
        {
            try
            {
                await _usuarioService.EliminarUsuarioAsync(id);
                return Json(new { success = true, message = "Empleado eliminado del sistema." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // PRIVATE HELPERS
        // ============================================================
        private async Task CargarViewBags()
        {
            // Solo roles de personal (Admin y Empleado)
            var todos = await _rolesRepository.GetAllAsync();
            var rolesPersonal = todos.Where(r => r.Nombre == AppRoles.Admin || r.Nombre == "Empleado").ToList();
            ViewBag.Roles = new SelectList(rolesPersonal, "RoleId", "Nombre");

            // Permisos agrupados con NivelPeligro para colorear la UI
            var permisos = await _context.Permisos.OrderBy(p => p.Modulo).ThenBy(p => p.PermisoId).ToListAsync();
            ViewBag.PermisosPorModulo = permisos.GroupBy(p => p.Modulo).OrderBy(g => g.Key);
        }
    }
}
