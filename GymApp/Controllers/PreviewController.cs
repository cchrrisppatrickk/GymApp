using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GymApp.Controllers
{
    [AllowAnonymous]
    public class PreviewController : Controller
    {
        public IActionResult Sidebar()
        {
            ViewData["Title"] = "Previsualización Sidebar";
            return View();
        }
    }
}
