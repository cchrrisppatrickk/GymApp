using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers
{
    // Solo los clientes pueden entrar aquí
    [Authorize(Roles = "Cliente")]
    public class ClienteHomeController : BaseController
    {
        public IActionResult Index()
        {
            // Aquí podrías enviar datos básicos como su estado de membresía
            ViewData["NombreUsuario"] = CurrentUserName;
            return View();
        }
    }
}