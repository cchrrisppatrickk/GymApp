using GymApp.Filters;
using GymApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers;

/// <summary>
/// Controlador REST exclusivo para peticiones del agente de IA (n8n).
/// 
/// Características de aislamiento:
///   - Hereda de <see cref="ControllerBase"/> (NO de BaseController) → sin vistas, sin cookies.
///   - [ApiController] → validación automática de modelos y respuestas JSON.
///   - [ApiKeyAuth]    → toda petición debe incluir el header X-API-KEY válido.
///   - [Route("api/agent")] → espacio de rutas separado del resto de la app MVC.
/// </summary>
[ApiController]
[Route("api/agent")]
[ApiKeyAuth]
public class ApiAgentController : ControllerBase
{
    private readonly IReporteService _reporteService;
    private readonly IUsuarioService _usuarioService;

    public ApiAgentController(IReporteService reporteService, IUsuarioService usuarioService)
    {
        _reporteService = reporteService;
        _usuarioService = usuarioService;
    }

    // -----------------------------------------------------------------------
    // PING — Endpoint de conectividad y validación de API Key
    // -----------------------------------------------------------------------

    /// <summary>
    /// Verifica la conectividad del canal n8n ↔ GymApp y la validez del API Key.
    /// GET /api/agent/ping
    /// Header requerido: X-API-KEY: &lt;valor configurado en ApiSettings:ApiKey&gt;
    /// </summary>
    /// <returns>200 OK con estado, mensaje y timestamp UTC.</returns>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new
        {
            status    = "success",
            message   = "API del Agente conectada correctamente",
            timestamp = DateTime.UtcNow
        });
    }
}
