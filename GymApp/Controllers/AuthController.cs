using GymApp.Models;
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
        public IActionResult Login()
        {
            // Si ya está logueado, ir al inicio
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(string dni, string password)
        {
            // 1. Validar usuario (Esto deberías mejorarlo con hash en el futuro)
            // Asumimos que tienes un método en tu servicio para buscar por DNI
            // var usuario = await _usuarioService.ValidarUsuario(dni, password); 

            // SIMULACIÓN RÁPIDA (Reemplaza con tu lógica real de servicio):
            // Aquí deberías buscar en la BD real.
            // if (usuario == null) { ViewBag.Error = "Credenciales incorrectas"; return View(); }

            // --- INICIO DE SIMULACIÓN PARA QUE FUNCIONE YA ---
            if (string.IsNullOrEmpty(dni)) // Solo valida que escriban algo
            {
                ViewBag.Error = "Ingrese credenciales";
                return View();
            }
            // --- FIN SIMULACIÓN ---

            // 2. Crear los CLAIMS (Datos del carnet de identidad digital)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dni), // Guardamos el DNI o Nombre
                new Claim(ClaimTypes.NameIdentifier, "1"), // ID del Usuario (Debe venir de la BD)
                new Claim(ClaimTypes.Role, "Admin") // Rol (Debe venir de la BD)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // "Recordarme"
            };

            // 3. Iniciar Sesión (Generar Cookie)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}