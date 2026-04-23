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
    private readonly IPagoService    _pagoService;

    public ApiAgentController(
        IReporteService reporteService,
        IUsuarioService usuarioService,
        IPagoService    pagoService)
    {
        _reporteService = reporteService;
        _usuarioService = usuarioService;
        _pagoService    = pagoService;
    }

    // -----------------------------------------------------------------------
    // PING — Conectividad y validación de API Key
    // -----------------------------------------------------------------------

    /// <summary>
    /// Verifica la conectividad n8n ↔ GymApp y la validez del API Key.
    /// GET /api/agent/ping
    /// </summary>
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

    // -----------------------------------------------------------------------
    // ESTADÍSTICAS — Herramientas analíticas para el agente
    // -----------------------------------------------------------------------

    /// <summary>
    /// Retorna estadísticas generales de usuarios: nuevos miembros, vencidos,
    /// por vencer en 7 días, deudores y membresías congeladas.
    /// GET /api/agent/estadisticas/usuarios
    /// </summary>
    [HttpGet("estadisticas/usuarios")]
    public async Task<IActionResult> GetEstadisticasUsuarios()
    {
        var resultado = await _reporteService.ObtenerEstadisticasUsuariosAsync();
        return Ok(resultado);
    }

    /// <summary>
    /// Retorna estadísticas financieras: ingresos mensuales (últimos 6 meses),
    /// ingresos semanales (últimas 4 semanas) y porcentajes de crecimiento.
    /// GET /api/agent/estadisticas/financieras
    /// </summary>
    [HttpGet("estadisticas/financieras")]
    public async Task<IActionResult> GetEstadisticasFinancieras()
    {
        var resultado = await _reporteService.ObtenerEstadisticasFinancierasAsync();
        return Ok(resultado);
    }

    // -----------------------------------------------------------------------
    // PAGOS — Consulta granular para el agente
    // -----------------------------------------------------------------------

    /// <summary>
    /// Retorna los 15 pagos más recientes proyectados en un payload liviano.
    /// Diseñado para no saturar la ventana de contexto del LLM (Ollama/n8n).
    /// GET /api/agent/pagos/recientes
    /// </summary>
    [HttpGet("pagos/recientes")]
    public async Task<IActionResult> GetPagosRecientes()
    {
        var todos = await _pagoService.ListarPagosAsync();

        var pagosFiltrados = todos
            .OrderByDescending(p => p.FechaPago)   // más recientes primero
            .Take(15)
            .Select(p => new
            {
                id           = p.PagoId,
                cliente      = p.NombreCliente,
                monto        = p.Monto,
                fecha        = p.FechaPago,
                metodoPago   = p.MetodoPago
            });

        return Ok(pagosFiltrados);
    }
}
