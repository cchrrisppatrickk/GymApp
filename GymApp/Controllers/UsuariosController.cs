using GymApp.Models;
using GymApp.Repositories;
using GymApp.Services;
using GymApp.ViewModels; // Importante
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymApp.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace GymApp.Controllers
{
    [Authorize(Policy = AppPoliticas.RequiereVerUsuarios)]
    public class UsuariosController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IGenericRepository<Role> _rolesRepository;
        private readonly IRestriccionService _restriccionService;
        private readonly IPeruApiService _peruApiService;

        public UsuariosController(IUsuarioService usuarioService, IGenericRepository<Role> rolesRepository, IRestriccionService restriccionService, IPeruApiService peruApiService)
        {
            _usuarioService = usuarioService;
            _rolesRepository = rolesRepository;
            _restriccionService = restriccionService;
            _peruApiService = peruApiService;
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

            // CARGAMOS LOS ROLES para el <select>
            var roles = await _rolesRepository.GetAllAsync();
            if (!User.IsInRole(AppRoles.Admin))
            {
                roles = roles.Where(r => r.Nombre != AppRoles.Admin).ToList();
            }
            ViewBag.Roles = new SelectList(roles, "RoleId", "Nombre");

            // Preparar listas para el CRM
            ViewBag.Generos = new SelectList(new[] { "Masculino", "Femenino", "Otro", "Prefiero no decirlo" });
            ViewBag.EstadosCiviles = new SelectList(new[] { "Soltero(a)", "Casado(a)", "Divorciado(a)", "Viudo(a)", "Conviviente" });
            ViewBag.Origenes = new SelectList(new[] { "Recomendación", "Facebook", "Instagram", "TikTok", "Publicidad Exterior", "Otro" });
            
            ViewData["CurrentFilter"] = buscar;
            ViewBag.Mes = mes;
            ViewBag.Anio = anio;
            ViewData["TituloListado"] = string.IsNullOrEmpty(buscar) ? "Listado de Usuarios" : $"Resultados para: {buscar}";

            return View(pagedResult);
        }


        // ==========================================
        // 6. VISTA DETALLES (CRM)
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _usuarioService.ObtenerDetallesCrmAsync(id);
            if (dto == null) return NotFound();

            return View(dto);
        }

        // ==========================================
        // 7. GESTIÓN DE RESTRICCIONES
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> AplicarRestriccion(int userId, string tipo, string descripcion)
        {
            if (!TienePermiso(AppPermisos.UsuariosEditar)) 
                return Json(new { success = false, message = "No tienes permiso para realizar esta acción." });

            try
            {
                int aplicadorId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _restriccionService.AplicarRestriccionAsync(userId, tipo, descripcion, aplicadorId);
                return Json(new { success = true, message = "Restricción aplicada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LevantarRestriccion(int id)
        {
            if (!TienePermiso(AppPermisos.UsuariosEditar)) 
                return Json(new { success = false, message = "No tienes permiso para realizar esta acción." });

            try
            {
                var success = await _restriccionService.LevantarRestriccionAsync(id);
                return Json(new { success = success, message = success ? "Restricción levantada." : "No se encontró la restricción." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
                NombreUsuario = u.NombreUsuario,
                Dni = u.Dni,
                Email = u.Email,
                Telefono = u.Telefono,
                Estado = u.Estado ?? false,
                Origen = u.Origen,
                ApellidoPaterno = u.ApellidoPaterno,
                ApellidoMaterno = u.ApellidoMaterno,
                EstadoCivil = u.EstadoCivil,
                Genero = u.Genero,
                Direccion = u.Direccion,
                WhatsApp = u.WhatsApp,
                FechaNacimiento = u.FechaNacimiento,
                Ocupacion = u.Ocupacion,
                Nota = u.Nota,
                PinAcceso = u.PinAcceso,
                PermisosSeleccionados = (await _usuarioService.ObtenerPermisosUsuarioAsync(id)).ToArray()
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
            if (model.UserId == 0)
            {
                if (!TienePermiso(AppPermisos.UsuariosCrear))
                {
                    return Json(new { success = false, message = "No tienes permiso para crear usuarios." });
                }
            }
            else
            {
                if (!TienePermiso(AppPermisos.UsuariosEditar))
                {
                    return Json(new { success = false, message = "No tienes permiso para editar usuarios." });
                }
            }


            // Prevención de Escalada de Privilegios
            var rolSeleccionado = await _rolesRepository.GetByIdAsync(model.RoleId);
            if (rolSeleccionado != null && rolSeleccionado.Nombre == AppRoles.Admin && !User.IsInRole(AppRoles.Admin))
            {
                ModelState.AddModelError("RoleId", "No tienes permisos para crear o modificar usuarios con rol de Administrador.");
            }

            if (model.UserId > 0)
            {
                var usuarioExistenteParaValidar = await _usuarioService.ObtenerPorIdAsync(model.UserId);
                if (usuarioExistenteParaValidar != null)
                {
                    var rolOriginal = await _rolesRepository.GetByIdAsync(usuarioExistenteParaValidar.RoleId);
                    if (rolOriginal != null && rolOriginal.Nombre == AppRoles.Admin && !User.IsInRole(AppRoles.Admin))
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
                        ApellidoPaterno = model.ApellidoPaterno,
                        ApellidoMaterno = model.ApellidoMaterno,
                        Dni = model.Dni,
                        Email = model.Email,
                        Telefono = model.Telefono,
                        WhatsApp = model.WhatsApp,
                        Direccion = model.Direccion,
                        Origen = model.Origen,
                        EstadoCivil = model.EstadoCivil,
                        Genero = model.Genero,
                        FechaNacimiento = model.FechaNacimiento,
                        Ocupacion = model.Ocupacion,
                        Nota = model.Nota,
                        // PinAcceso = model.PinAcceso, // Se generará automáticamente en el Service
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
                    usuarioExistente.ApellidoPaterno = model.ApellidoPaterno;
                    usuarioExistente.ApellidoMaterno = model.ApellidoMaterno;
                    
                    // Solo actualizamos NombreUsuario si viene un valor no vacío (para no dejarlo nulo)
                    if (!string.IsNullOrEmpty(model.NombreUsuario))
                    {
                        usuarioExistente.NombreUsuario = model.NombreUsuario;
                    }
                    
                    usuarioExistente.Dni = model.Dni;
                    usuarioExistente.Email = model.Email;
                    usuarioExistente.Telefono = model.Telefono;
                    usuarioExistente.WhatsApp = model.WhatsApp;
                    usuarioExistente.Direccion = model.Direccion;
                    usuarioExistente.Origen = model.Origen;
                    usuarioExistente.EstadoCivil = model.EstadoCivil;
                    usuarioExistente.Genero = model.Genero;
                    usuarioExistente.FechaNacimiento = model.FechaNacimiento;
                    usuarioExistente.Ocupacion = model.Ocupacion;
                    usuarioExistente.Nota = model.Nota;
                    // usuarioExistente.PinAcceso = model.PinAcceso; // El PIN se gestiona automáticamente
                    usuarioExistente.Estado = model.Estado;

                    // Auditoría
                    usuarioExistente.FechaUltimaModificacion = DateTime.Now;
                    usuarioExistente.ModificadoPorId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

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
        [Authorize(Policy = AppPoliticas.RequiereEliminarUsuarios)]
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

        [HttpPost]
        public async Task<IActionResult> RegenerarPin(int id)
        {
            if (!TienePermiso(AppPermisos.UsuariosEditar))
                return Json(new { success = false, message = "No tienes permiso para esta acción." });

            try
            {
                string nuevoPin = await _usuarioService.RegenerarPinAsync(id);
                return Json(new { success = true, pin = nuevoPin, message = "PIN regenerado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConsultarDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni) || dni.Length != 8)
                return Json(new { success = false, message = "DNI inválido" });

            try
            {
                var data = await _peruApiService.ConsultarDniAsync(dni);
                if (data != null && data.Code == "200")
                {
                    return Json(new { success = true, data = data });
                }
                
                // Mostrar el mensaje real de error de la API (ej: "API Key inválida")
                var errorMsg = data?.Mensaje ?? "No se encontraron datos para el DNI ingresado.";
                return Json(new { success = false, message = errorMsg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}