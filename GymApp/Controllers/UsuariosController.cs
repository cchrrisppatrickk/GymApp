using GymApp.Models;
using GymApp.Repositories;
using GymApp.Services;
using GymApp.ViewModels; // Importante
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymApp.Controllers
{
    [Authorize(Roles = "Admin,Empleado")]
    public class UsuariosController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IGenericRepository<Role> _rolesRepository;

        public UsuariosController(IUsuarioService usuarioService, IGenericRepository<Role> rolesRepository)
        {
            _usuarioService = usuarioService;
            _rolesRepository = rolesRepository;
        }

        // ==========================================
        // 1. VISTA PRINCIPAL (INDEX) - PAGINADA
        // ==========================================
        public async Task<IActionResult> Index(string? buscar, int? mes, int? anio, int pagina = 1)
        {
            if (string.IsNullOrEmpty(buscar) && (!mes.HasValue || !anio.HasValue))
            {
                mes = DateTime.Now.Month;
                anio = DateTime.Now.Year;
            }

            var pagedResult = await _usuarioService.ObtenerUsuariosPaginadosAsync(buscar, pagina, mes, anio);

            // CARGAMOS LOS ROLES AQUÍ para enviarlos a la vista y llenar el <select> del Modal
            var roles = await _rolesRepository.GetAllAsync();
            ViewBag.Roles = new SelectList(roles, "RoleId", "Nombre");
            
            ViewData["CurrentFilter"] = buscar;
            ViewBag.Mes = mes;
            ViewBag.Anio = anio;
            ViewData["TituloListado"] = string.IsNullOrEmpty(buscar) ? "Listado de Usuarios" : $"Resultados para: {buscar}";

            return View(pagedResult);
        }


        // ==========================================
        // 6. VISTA DETALLES (PREMIUM)
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }


        // ==========================================
        // 2. OBTENER UN USUARIO (AJAX)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var u = await _usuarioService.ObtenerPorIdAsync(id);
            if (u == null) return NotFound();

            var model = new UsuarioViewModel
            {
                UserId = u.UserId,
                RoleId = u.RoleId,
                NombreCompleto = u.NombreCompleto,
                NombreUsuario = u.NombreUsuario, // <--- NUEVO CAMPO
                Dni = u.Dni,
                Email = u.Email,
                Telefono = u.Telefono,
                Estado = u.Estado ?? false,
                // Password se deja vacío por seguridad
            };

            return Json(new { success = true, data = model });
        }

        // ==========================================
        // 3. GUARDAR (CREAR / EDITAR)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromForm] UsuarioViewModel model)
        {
            // OJO: ModelState.IsValid podría fallar si tienes validaciones Required en el ViewModel. 
            // Como ya quitamos los Required de Password y Usuario en el paso 1, esto pasará bien.
            // Validamos solo lo básico (DNI, Nombre, Rol)
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Datos incompletos", errors = errors });
            }
            try
            {
                // --- CREAR ---
                if (model.UserId == 0)
                {
                    // ELIMINAMOS O COMENTAMOS ESTA VALIDACIÓN ANTIGUA:
                    // if (string.IsNullOrEmpty(model.Password)) return Json(...) 
                    // Ya no es necesaria porque el Service se encarga.

                    var nuevoUsuario = new Usuario
                    {
                        RoleId = model.RoleId,
                        NombreCompleto = model.NombreCompleto,
                        Dni = model.Dni,
                        Email = model.Email,
                        Telefono = model.Telefono,
                        Estado = true,

                        // LÓGICA AUTOMÁTICA:
                        // Si el modelo viene sin usuario, usamos el DNI
                        NombreUsuario = string.IsNullOrEmpty(model.NombreUsuario) ? model.Dni : model.NombreUsuario
                    };

                    // Si el password viene vacío, usamos el DNI
                    string passwordFinal = string.IsNullOrEmpty(model.Password) ? model.Dni : model.Password;

                    await _usuarioService.CrearUsuarioAsync(nuevoUsuario, passwordFinal, model.FotoArchivo);
                    return Json(new { success = true, message = "Usuario creado. Acceso con DNI." });
                }
                // --- EDITAR ---
                else
                {
                    var usuarioExistente = await _usuarioService.ObtenerPorIdAsync(model.UserId);
                    if (usuarioExistente == null) return Json(new { success = false, message = "Usuario no encontrado." });

                    // Mapeo de actualizaciones
                    usuarioExistente.RoleId = model.RoleId;
                    usuarioExistente.NombreCompleto = model.NombreCompleto;
                    usuarioExistente.NombreUsuario = model.NombreUsuario; // <--- Mapeo
                    usuarioExistente.Dni = model.Dni;
                    usuarioExistente.Email = model.Email;
                    usuarioExistente.Telefono = model.Telefono;
                    usuarioExistente.Estado = model.Estado;

                    // Solo si el usuario escribió algo en el campo Password, lo actualizamos
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        usuarioExistente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    }

                    await _usuarioService.ActualizarUsuarioAsync(usuarioExistente, model.FotoArchivo);
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