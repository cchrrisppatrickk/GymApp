using System.Threading.Tasks;
using GymApp.Services;
using GymApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymApp.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymApp.Controllers;

[Authorize(Policy = AppPoliticas.RequiereVerPasesDiarios)]
public class PasesDiariosController : BaseController
{
    private readonly IPaseDiarioService _paseDiarioService;
    private readonly ITurnoService _turnoService;
    private readonly IUsuarioService _usuarioService;

    public PasesDiariosController(IPaseDiarioService paseDiarioService, ITurnoService turnoService, IUsuarioService usuarioService)
    {
        _paseDiarioService = paseDiarioService;
        _turnoService = turnoService;
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var pases = await _paseDiarioService.ListarPasesAsync();
        return View(pases);
    }

    [HttpGet]
    public async Task<IActionResult> Registrar()
    {
        var turnos = await _turnoService.ObtenerTodosAsync();
        var usuarios = await _usuarioService.ObtenerTodosAsync();

        ViewBag.Turnos = new SelectList(turnos, "TurnoId", "Nombre");
        ViewBag.Usuarios = new SelectList(usuarios, "UserId", "NombreCompleto");

        return View(new PaseDiarioCreateDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(PaseDiarioCreateDTO model)
    {
        if (!ModelState.IsValid)
        {
            var turnos = await _turnoService.ObtenerTodosAsync();
            var usuarios = await _usuarioService.ObtenerTodosAsync();
            ViewBag.Turnos = new SelectList(turnos, "TurnoId", "Nombre", model.TurnoId);
            ViewBag.Usuarios = new SelectList(usuarios, "UserId", "NombreCompleto", model.UserId);
            return View(model);
        }

        await _paseDiarioService.RegistrarPaseAsync(model, CurrentUserId);
        TempData["SuccessMessage"] = "Pase Diario registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = AppPoliticas.RequiereEliminarPasesDiarios)]
    public async Task<IActionResult> EliminarFisicamente(int id)
    {
        try
        {
            await _paseDiarioService.EliminarFisicamenteAsync(id);
            return Json(new { success = true, message = "Pase Diario eliminado físicamente." });
        }
        catch (System.Exception ex)
        {
            return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
        }
    }
}
