using GymApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public AuthController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya tiene brazalete (Cookie), lo mandamos directo al Dashboard
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(string dni, string password)
        {
            // 1. Validar campos vacíos
            if (string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Por favor, ingrese DNI y contraseña.";
                return View();
            }

            try
            {
                // 2. Lógica Real (Usando tu Servicio y BCrypt interno)
                var usuario = await _usuarioService.ValidarLoginAsync(dni, password);

                if (usuario == null)
                {
                    ViewBag.Error = "Credenciales incorrectas o usuario inactivo.";
                    return View();
                }

                // 3. Crear los CLAIMS (La información dentro del brazalete/cookie)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                    new Claim(ClaimTypes.NameIdentifier, usuario.UserId.ToString()), // CRÍTICO: Aquí guardamos el ID
                    new Claim(ClaimTypes.Role, usuario.Role.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Email ?? "")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 4. Configurar la Cookie
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = true, // "Mantener sesión iniciada"
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(60) // Expira en 1 hora
                };

                // 5. Firmar y Entregar la Cookie
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // 6. Redirigir al Panel Principal
                await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

                // === NUEVA LÓGICA DE REDIRECCIÓN ===
                if (usuario.Role.Nombre == "Admin" || usuario.Role.Nombre == "Empleado")
                {
                    return RedirectToAction("Index", "Home"); // Dashboard completo
                }
                else if (usuario.Role.Nombre == "Cliente")
                {
                    return RedirectToAction("Index", "ClienteHome"); // Vista limitada
                }
                else
                {
                    return RedirectToAction("Index", "Home"); // Default
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error al intentar ingresar: " + ex.Message;
                return View();
            }
        }

        // GET: /Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: /Auth/AccesoDenegado
        public IActionResult AccesoDenegado()
        {
            return View(); // Crea una vista simple que diga "No tienes permisos"
        }
    }
}