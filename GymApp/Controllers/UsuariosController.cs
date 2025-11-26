using GymApp.Models;
using GymApp.Services;
using GymApp.Repositories;
using GymApp.ViewModels; // Importante
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymApp.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IGenericRepository<Role> _rolesRepository;

        public UsuariosController(IUsuarioService usuarioService, IGenericRepository<Role> rolesRepository)
        {
            _usuarioService = usuarioService;
            _rolesRepository = rolesRepository;
        }

        // ==========================================
        // 1. VISTA PRINCIPAL (INDEX)
        // ==========================================
        public async Task<IActionResult> Index(string tipo = "Todos")
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();

            // Filtrado básico para la vista inicial
            if (tipo == "Cliente")
            {
                usuarios = usuarios.Where(u => u.Role.Nombre == "Cliente");
                ViewData["TituloListado"] = "Listado de Clientes";
            }
            else if (tipo == "Personal")
            {
                usuarios = usuarios.Where(u => u.Role.Nombre != "Cliente");
                ViewData["TituloListado"] = "Listado de Personal";
            }
            else
            {
                ViewData["TituloListado"] = "Todos los Usuarios";
            }

            // CARGAMOS LOS ROLES AQUÍ para enviarlos a la vista y llenar el <select> del Modal
            var roles = await _rolesRepository.GetAllAsync();
            ViewBag.Roles = new SelectList(roles, "RoleId", "Nombre");
            ViewBag.FiltroActual = tipo;

            // Mapeamos a ViewModel para la tabla
            var listadoViewModels = usuarios.Select(u => new UsuarioViewModel
            {
                UserId = u.UserId,
                NombreCompleto = u.NombreCompleto,
                Dni = u.Dni,
                Email = u.Email,
                Telefono = u.Telefono,
                NombreRol = u.Role?.Nombre,
                // CORRECCIÓN AQUÍ: (u.Estado ?? false)
                // Si u.Estado es nulo, se usará 'false' automáticamente.
                Estado = u.Estado ?? false,
                RoleId = u.RoleId
            }).ToList();

            return View(listadoViewModels);
        }

        // ==========================================
        // 2. OBTENER UN USUARIO (AJAX)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var u = await _usuarioService.ObtenerPorIdAsync(id);
            if (u == null) return NotFound();

            // Retornamos solo lo necesario para el formulario
            var model = new UsuarioViewModel
            {
                UserId = u.UserId,
                RoleId = u.RoleId,
                NombreCompleto = u.NombreCompleto,
                Dni = u.Dni,
                Email = u.Email,
                Telefono = u.Telefono,
                // CORRECCIÓN AQUÍ TAMBIÉN:
                Estado = u.Estado ?? false,
            };

            return Json(new { success = true, data = model });
        }

        // ==========================================
        // 3. GUARDAR (CREAR / EDITAR)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Datos inválidos", errors = errors });
            }

            try
            {
                // CASO 1: CREAR (UserId es 0)
                if (model.UserId == 0)
                {
                    if (string.IsNullOrEmpty(model.Password))
                        return Json(new { success = false, message = "La contraseña es obligatoria para nuevos usuarios." });

                    var nuevoUsuario = new Usuario
                    {
                        RoleId = model.RoleId,
                        NombreCompleto = model.NombreCompleto,
                        Dni = model.Dni,
                        Email = model.Email,
                        Telefono = model.Telefono,
                        Estado = true,
                        FechaRegistro = DateTime.Now
                    };

                    await _usuarioService.CrearUsuarioAsync(nuevoUsuario, model.Password);
                    return Json(new { success = true, message = "Usuario creado exitosamente." });
                }
                // CASO 2: EDITAR
                else
                {
                    var usuarioExistente = await _usuarioService.ObtenerPorIdAsync(model.UserId);
                    if (usuarioExistente == null) return Json(new { success = false, message = "Usuario no encontrado." });

                    usuarioExistente.RoleId = model.RoleId;
                    usuarioExistente.NombreCompleto = model.NombreCompleto;
                    usuarioExistente.Dni = model.Dni;
                    usuarioExistente.Email = model.Email;
                    usuarioExistente.Telefono = model.Telefono;
                    usuarioExistente.Estado = model.Estado; // bool a bool? funciona implícitamente, aquí no da error

                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        // Lógica de cambio de contraseña
                    }

                    await _usuarioService.ActualizarUsuarioAsync(usuarioExistente);
                    return Json(new { success = true, message = "Usuario actualizado correctamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // 4. ELIMINAR (AJAX)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var resultado = await _usuarioService.EliminarUsuarioAsync(id);
                if (!resultado) return Json(new { success = false, message = "No se pudo eliminar el usuario." });

                return Json(new { success = true, message = "Usuario eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // 5. OBTENER QR (IMAGEN)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ObtenerQR(int id)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);

            if (usuario == null || usuario.CodigoQr == null || usuario.CodigoQr == Guid.Empty)
            {
                return NotFound();
            }

            byte[] imagenBytes = _usuarioService.GenerarImagenQR(usuario.CodigoQr.Value);
            return File(imagenBytes, "image/png");
        }
    }
}