using GymApp.Models;
using GymApp.Repositories;
using GymApp.Services;
using GymApp.ViewModels; // Importante
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace GymApp.Controllers
{
    [Authorize(Policy = "RequiereVerUsuarios")]
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

            // CARGAMOS LOS ROLES para el <select> del Modal de socios
            var roles = await _rolesRepository.GetAllAsync();
            if (!User.IsInRole("Admin"))
            {
                roles = roles.Where(r => r.Nombre != "Admin").ToList();
            }
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
                PermisosSeleccionados = (await _usuarioService.ObtenerPermisosUsuarioAsync(id)).ToArray()
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
            // Prevención de Escalada de Privilegios
            var rolSeleccionado = await _rolesRepository.GetByIdAsync(model.RoleId);
            if (rolSeleccionado != null && rolSeleccionado.Nombre == "Admin" && !User.IsInRole("Admin"))
            {
                ModelState.AddModelError("RoleId", "No tienes permisos para crear o modificar usuarios con rol de Administrador.");
            }

            if (model.UserId > 0)
            {
                var usuarioExistenteParaValidar = await _usuarioService.ObtenerPorIdAsync(model.UserId);
                if (usuarioExistenteParaValidar != null)
                {
                    var rolOriginal = await _rolesRepository.GetByIdAsync(usuarioExistenteParaValidar.RoleId);
                    if (rolOriginal != null && rolOriginal.Nombre == "Admin" && !User.IsInRole("Admin"))
                    {
                        ModelState.AddModelError("RoleId", "No tienes permisos para editar a un Administrador.");
                    }
                }
            }

            // OJO: ModelState.IsValid podría fallar si tienes validaciones Required en el ViewModel. 
            // Como ya quitamos los Required de Password y Usuario en el paso 1, esto pasará bien.
            // Validamos solo lo básico (DNI, Nombre, Rol)
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Datos incompletos o permisos insuficientes", errors = errors });
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
                        NombreUsuario = model.NombreUsuario
                    };

                    var usuarioCreado = await _usuarioService.CrearUsuarioAsync(nuevoUsuario, model.Password, model.FotoArchivo, model.FotoBase64);
                    
                    // Actualizamos siempre, incluso si es null (para vaciar si desmarcan todo)
                    await _usuarioService.ActualizarPermisosUsuarioAsync(usuarioCreado.UserId, model.PermisosSeleccionados ?? Array.Empty<string>());

                    return Json(new { success = true, message = "Usuario creado exitosamente." });
                }
                // --- EDITAR ---
                else
                {
                    var usuarioExistente = await _usuarioService.ObtenerPorIdAsync(model.UserId);
                    if (usuarioExistente == null) return Json(new { success = false, message = "Usuario no encontrado." });

                    // Mapeo de actualizaciones
                    usuarioExistente.RoleId = model.RoleId;
                    usuarioExistente.NombreCompleto = model.NombreCompleto;
                    
                    // Solo actualizamos NombreUsuario si viene un valor no vacío (para no dejarlo nulo)
                    if (!string.IsNullOrEmpty(model.NombreUsuario))
                    {
                        usuarioExistente.NombreUsuario = model.NombreUsuario;
                    }
                    
                    usuarioExistente.Dni = model.Dni;
                    usuarioExistente.Email = model.Email;
                    usuarioExistente.Telefono = model.Telefono;
                    usuarioExistente.Estado = model.Estado;

                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        usuarioExistente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    }

                    await _usuarioService.ActualizarUsuarioAsync(usuarioExistente, model.FotoArchivo, model.FotoBase64);
                    
                    // Actualizamos siempre, incluso si es null (para vaciar si desmarcan todo)
                    await _usuarioService.ActualizarPermisosUsuarioAsync(usuarioExistente.UserId, model.PermisosSeleccionados ?? Array.Empty<string>());

                    return Json(new { success = true, message = "Usuario actualizado correctamente." });
                }
            }
            catch (Exception ex)
            {
                var fullMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    fullMessage += " | Inner: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        fullMessage += " | Inner2: " + ex.InnerException.InnerException.Message;
                    }
                }
                return Json(new { success = false, message = fullMessage });
            }
        }

        // ==========================================
        // 4. ELIMINAR (AJAX)
        // ==========================================
        [HttpPost]
        [Authorize(Policy = "RequiereEliminarUsuarios")]
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