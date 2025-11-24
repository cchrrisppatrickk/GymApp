using GymApp.Models;
using GymApp.Services;
using GymApp.Repositories; // Necesario para obtener los Roles
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Necesario para SelectList
using System;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    public class UsuariosController : Controller
    {
        // Inyectamos el Servicio (Lógica de Negocio)
        private readonly IUsuarioService _usuarioService;

        // Inyectamos el Repositorio de Roles (Para llenar los DropDownList)
        private readonly IGenericRepository<Role> _rolesRepository;

        public UsuariosController(IUsuarioService usuarioService, IGenericRepository<Role> rolesRepository)
        {
            _usuarioService = usuarioService;
            _rolesRepository = rolesRepository;
        }

        // ==========================================
        // 1. LISTADO (INDEX)
        // ==========================================
        public async Task<IActionResult> Index(string tipo = "Cliente") // Por defecto muestra clientes
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();

            if (tipo == "Cliente")
            {
                // Filtramos donde el Rol sea exactamente "Cliente"
                usuarios = usuarios.Where(u => u.Role.Nombre == "Cliente");
                ViewData["TituloListado"] = "Listado de Clientes";
            }
            else
            {
                // Filtramos todo lo que NO sea Cliente (Admin, Portero, etc.)
                usuarios = usuarios.Where(u => u.Role.Nombre != "Cliente");
                ViewData["TituloListado"] = "Listado de Personal";
            }

            return View(usuarios);
        }

        // ==========================================
        // 2. DETALLES (PERFIL)
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _usuarioService.ObtenerPorIdAsync(id.Value);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // ==========================================
        // 3. CREAR (CREATE)
        // ==========================================

        // GET: Muestra el formulario vacío
        public async Task<IActionResult> Create()
        {
            // Cargar la lista de roles para el <select> de la vista
            var roles = await _rolesRepository.GetAllAsync();
            ViewData["RoleId"] = new SelectList(roles, "RoleId", "Nombre");
            return View();
        }

        // POST: Recibe los datos y llama al servicio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NombreCompleto,Dni,Email,Telefono,RoleId")] Usuario usuario, string passwordRaw)
        {
            // Validamos que se haya ingresado una contraseña
            if (string.IsNullOrEmpty(passwordRaw))
            {
                ModelState.AddModelError("PasswordHash", "La contraseña es obligatoria para nuevos usuarios.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // El servicio se encarga de Hashear password, generar QR y validar DNI
                    await _usuarioService.CrearUsuarioAsync(usuario, passwordRaw);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Capturamos errores de negocio (ej: DNI duplicado)
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            // Si falló, recargamos la lista de roles y devolvemos la vista con los errores
            var roles = await _rolesRepository.GetAllAsync();
            ViewData["RoleId"] = new SelectList(roles, "RoleId", "Nombre", usuario.RoleId);
            return View(usuario);
        }

        // ==========================================
        // 4. EDITAR (EDIT)
        // ==========================================

        // GET: Muestra el formulario con datos existentes
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var usuario = await _usuarioService.ObtenerPorIdAsync(id.Value);
            if (usuario == null) return NotFound();

            var roles = await _rolesRepository.GetAllAsync();
            ViewData["RoleId"] = new SelectList(roles, "RoleId", "Nombre", usuario.RoleId);

            // IMPORTANTE: Si la petición es AJAX (viene del modal), devolvemos PartialView
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", usuario);
            }

            return View(usuario);
        }

        // POST: Guarda los cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,RoleId,NombreCompleto,Dni,Telefono,Email,CodigoQr,FechaRegistro,Estado,PasswordHash")] Usuario usuario)
        {
            if (id != usuario.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // NOTA: Aquí no estamos cambiando la contraseña. 
                    // Eso requeriría una lógica separada para no sobrescribir el Hash con nulo.
                    await _usuarioService.ActualizarUsuarioAsync(usuario);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Error al actualizar: " + ex.Message);
                }
            }

            var roles = await _rolesRepository.GetAllAsync();
            ViewData["RoleId"] = new SelectList(roles, "RoleId", "Nombre", usuario.RoleId);
            return View(usuario);
        }

        // ==========================================
        // 5. ELIMINAR (DELETE)
        // ==========================================

        // GET: Pregunta "¿Estás seguro?"
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _usuarioService.ObtenerPorIdAsync(id.Value);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Confirma y elimina
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _usuarioService.EliminarUsuarioAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 6. FUNCIONALIDAD QR
        // ==========================================

        // GET: Usuarios/ObtenerQR/5
        // Retorna la imagen PNG del QR
        public async Task<IActionResult> ObtenerQR(int id)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);

            // CORRECCIÓN AQUI:
            // 1. Verificamos si el objeto usuario es nulo
            // 2. Verificamos si la propiedad CodigoQr es nula ("usuario.CodigoQr == null")
            // 3. Verificamos si es Guid.Empty (aunque esto es menos probable si es nullable)
            if (usuario == null || usuario.CodigoQr == null || usuario.CodigoQr == Guid.Empty)
            {
                return NotFound();
            }

            // CORRECCIÓN AQUI:
            // Usamos .Value para extraer el Guid puro del Guid? (Nullable)
            byte[] imagenBytes = _usuarioService.GenerarImagenQR(usuario.CodigoQr.Value);

            // "image/png" hace que el navegador sepa que es una imagen
            return File(imagenBytes, "image/png");
        }
    }
}