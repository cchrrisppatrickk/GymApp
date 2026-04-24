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
    private readonly IMembresiaService _membresiaService;

    public ApiAgentController(
        IReporteService reporteService,
        IUsuarioService usuarioService,
        IPagoService    pagoService,
        IMembresiaService membresiaService)
    {
        _reporteService = reporteService;
        _usuarioService = usuarioService;
        _pagoService    = pagoService;
        _membresiaService = membresiaService;
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

    // -----------------------------------------------------------------------
    // USUARIOS — Dominio de Identidad y Búsqueda para el Agente IA
    // -----------------------------------------------------------------------

    /// <summary>
    /// Busca usuarios por nombre (coincidencia parcial) o DNI (coincidencia exacta).
    /// Retorna un DTO ultra-ligero sin datos sensibles (sin PasswordHash, FotoUrl, etc.).
    /// GET /api/agent/usuarios/buscar?q={termino}
    /// </summary>
    [HttpGet("usuarios/buscar")]
    public async Task<IActionResult> BuscarUsuarios([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "El parámetro 'q' es obligatorio y no puede estar vacío." });

        var resultado = await _usuarioService.BuscarParaAgenteAsync(q.Trim());
        return Ok(resultado);
    }

    /// <summary>
    /// Devuelve los usuarios registrados en los últimos N días (por defecto 7).
    /// Útil para el agente cuando necesita detectar nuevos miembros recientes.
    /// GET /api/agent/usuarios/nuevos?dias=7
    /// </summary>
    [HttpGet("usuarios/nuevos")]
    public async Task<IActionResult> GetUsuariosNuevos([FromQuery] int dias = 7)
    {
        if (dias <= 0)
            return BadRequest(new { error = "El parámetro 'dias' debe ser un número positivo." });

        var resultado = await _usuarioService.ObtenerRecientesParaAgenteAsync(dias);
        return Ok(resultado);
    }

    /// <summary>
    /// Devuelve los usuarios cuya fecha de registro coincide exactamente con el día indicado
    /// (ignora la hora). Formato esperado: yyyy-MM-dd.
    /// GET /api/agent/usuarios/fecha?fecha=2025-04-24
    /// </summary>
    [HttpGet("usuarios/fecha")]
    public async Task<IActionResult> GetUsuariosPorFecha([FromQuery] DateTime fecha)
    {
        var resultado = await _usuarioService.ObtenerPorFechaExactaParaAgenteAsync(fecha);
        return Ok(resultado);
    }

    // -----------------------------------------------------------------------
    // MEMBRESÍAS — Dominio de Estado y Servicio para el Agente IA
    // -----------------------------------------------------------------------

    /// <summary>
    /// Consulta el estado de la membresía activa o congelada de un usuario.
    /// GET /api/agent/membresias/usuario/{userId}/activa
    /// </summary>
    [HttpGet("membresias/usuario/{userId}/activa")]
    public async Task<IActionResult> GetMembresiaActiva(int userId)
    {
        var resultado = await _membresiaService.ObtenerActivaParaAgenteAsync(userId);
        if (resultado == null)
            return NotFound(new { error = "No hay membresía activa para este usuario" });

        return Ok(resultado);
    }

    /// <summary>
    /// Retorna el historial de membresías de un usuario.
    /// GET /api/agent/membresias/usuario/{userId}/historial
    /// </summary>
    [HttpGet("membresias/usuario/{userId}/historial")]
    public async Task<IActionResult> GetHistorialMembresias(int userId)
    {
        var resultado = await _membresiaService.ObtenerHistorialParaAgenteAsync(userId);
        return Ok(resultado);
    }

    /// <summary>
    /// Retorna membresías críticas: vencidas recientemente o por vencer.
    /// GET /api/agent/membresias/alertas?dias=7
    /// </summary>
    [HttpGet("membresias/alertas")]
    public async Task<IActionResult> GetAlertasMembresias([FromQuery] int dias = 7)
    {
        if (dias <= 0)
            return BadRequest(new { error = "El parámetro 'dias' debe ser un número positivo." });

        var resultado = await _membresiaService.ObtenerAlertasParaAgenteAsync(dias);
        return Ok(resultado);
    }
}
