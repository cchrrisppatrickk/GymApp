using GymApp.Filters;
using GymApp.Services;
using GymApp.ViewModels.ApiAgent;
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
    /// Retorna los 5 pagos más recientes registrados en el sistema.
    /// GET /api/agent/pagos/recientes
    /// </summary>
    [HttpGet("pagos/recientes")]
    public async Task<IActionResult> GetPagosRecientes()
    {
        // Optimizamos: Pedimos pagos de los últimos 30 días para obtener los más recientes sin procesar toda la historia
        var desde = DateTime.Today.AddDays(-30);
        var todos = await _pagoService.ObtenerPagosPorRangoParaAgenteAsync(desde, DateTime.Now);

        var pagosFiltrados = todos
            .OrderByDescending(p => p.Fecha)
            .Take(5)
            .Select(p => new
            {
                id         = p.Id,
                cliente    = p.NombreCliente,
                monto      = p.Monto,
                fecha      = p.Fecha.ToString("s"),
                metodoPago = p.MetodoPago
            })
            .ToList();

        return Ok(pagosFiltrados);
    }

    /// <summary>
    /// Retorna un resumen de todos los pagos registrados durante el día de hoy.
    /// GET /api/agent/pagos/hoy
    /// </summary>
    [HttpGet("pagos/hoy")]
    public async Task<IActionResult> GetPagosHoy()
    {
        var hoy = DateTime.Today;
        var finDelDia = hoy.AddDays(1).AddTicks(-1);

        var listaPagos = await _pagoService.ObtenerPagosPorRangoParaAgenteAsync(hoy, finDelDia);

        var pagosHoy = listaPagos
            .Select(p => new
            {
                id         = p.Id,
                cliente    = p.NombreCliente,
                monto      = p.Monto,
                fecha      = p.Fecha.ToString("s"),
                metodoPago = p.MetodoPago
            })
            .ToList();

        return Ok(new
        {
            fecha    = hoy.ToString("dd/MM/yyyy"),
            totalMonto = pagosHoy.Sum(p => p.monto),
            cantidad = pagosHoy.Count,
            pagos    = pagosHoy
        });
    }

    /// <summary>
    /// Retorna la lista completa de deudores o membresías con pagos pendientes.
    /// GET /api/agent/pagos/deudores
    /// </summary>
    [HttpGet("pagos/deudores")]
    public async Task<IActionResult> GetDeudores()
    {
        var resultado = await _reporteService.ObtenerListaDeudoresAsync();
        return Ok(resultado);
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
    /// Soporta ID directo o búsqueda por nombre/DNI (?q=...)
    /// GET /api/agent/membresias/usuario/activa
    /// </summary>
    [HttpGet("membresias/usuario/activa")]
    public async Task<IActionResult> GetMembresiaActiva([FromQuery] int? userId, [FromQuery] string? q)
    {
        var (targetId, action) = await ResolverUsuarioOAmbiguidad(userId, q);
        if (action != null) return action;

        var resultado = await _membresiaService.ObtenerActivaParaAgenteAsync(targetId.Value);
        if (resultado == null)
            return NotFound(new { error = "No hay membresía activa para este usuario" });

        return Ok(resultado);
    }

    /// <summary>
    /// Retorna el historial de membresías de un usuario.
    /// Soporta ID directo o búsqueda por nombre/DNI (?q=...)
    /// GET /api/agent/membresias/usuario/historial
    /// </summary>
    [HttpGet("membresias/usuario/historial")]
    public async Task<IActionResult> GetHistorialMembresias([FromQuery] int? userId, [FromQuery] string? q)
    {
        var (targetId, action) = await ResolverUsuarioOAmbiguidad(userId, q);
        if (action != null) return action;

        var resultado = await _membresiaService.ObtenerHistorialParaAgenteAsync(targetId.Value);
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

    // -----------------------------------------------------------------------
    // PAGOS Y DEUDAS — Dominio Financiero Granular para el Agente IA
    // -----------------------------------------------------------------------

    /// <summary>
    /// Retorna el consolidado de deuda de un cliente y la cantidad de membresías adeudadas.
    /// Soporta ID directo o búsqueda por nombre/DNI (?q=...)
    /// GET /api/agent/pagos/deuda
    /// </summary>
    [HttpGet("pagos/deuda")]
    public async Task<IActionResult> GetDeudaCliente([FromQuery] int? userId, [FromQuery] string? q)
    {
        var (targetId, action) = await ResolverUsuarioOAmbiguidad(userId, q);
        if (action != null) return action;

        var resultado = await _pagoService.ObtenerDeudaTotalParaAgenteAsync(targetId.Value);
        return Ok(resultado);
    }

    /// <summary>
    /// Retorna el historial de pagos de un cliente específico.
    /// Soporta ID directo o búsqueda por nombre/DNI (?q=...)
    /// GET /api/agent/pagos/usuario
    /// </summary>
    [HttpGet("pagos/usuario")]
    public async Task<IActionResult> GetHistorialPagosUsuario([FromQuery] int? userId, [FromQuery] string? q)
    {
        var (targetId, action) = await ResolverUsuarioOAmbiguidad(userId, q);
        if (action != null) return action;

        var resultado = await _pagoService.ObtenerHistorialUsuarioParaAgenteAsync(targetId.Value);
        
        // Retornamos lista vacía si no hay para no romper el flujo
        if (resultado == null || !resultado.Any())
            return Ok(new List<PagoAgenteDTO>());

        return Ok(resultado);
    }

    /// <summary>
    /// Busca todos los pagos realizados en un rango de fechas.
    /// GET /api/agent/pagos/rango?inicio=2025-01-01&fin=2025-12-31
    /// </summary>
    [HttpGet("pagos/rango")]
    public async Task<IActionResult> GetPagosPorRango([FromQuery] DateTime inicio, [FromQuery] DateTime fin)
    {
        if (fin < inicio)
            return BadRequest(new { error = "La fecha de fin no puede ser menor a la de inicio." });

        var resultado = await _pagoService.ObtenerPagosPorRangoParaAgenteAsync(inicio, fin);
        return Ok(resultado);
    }
    // -----------------------------------------------------------------------
    // MÉTODOS PRIVADOS DE APOYO
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lógica centralizada para resolver un usuario ya sea por ID o por búsqueda (Nombre/DNI).
    /// Si hay ambigüedad (múltiples resultados), retorna un BadRequest con la lista de sugerencias.
    /// </summary>
    private async Task<(int? id, IActionResult? action)> ResolverUsuarioOAmbiguidad(int? userId, string? q)
    {
        // 1. Si ya tenemos el ID, verificamos que existe
        if (userId.HasValue)
        {
            var existe = await _usuarioService.ObtenerPorIdAsync(userId.Value);
            if (existe == null) 
                return (null, NotFound(new { error = $"Usuario con ID {userId} no encontrado." }));
            
            return (userId, null);
        }

        // 2. Si no hay ID ni búsqueda, error
        if (string.IsNullOrWhiteSpace(q))
        {
            return (null, BadRequest(new { error = "Debes proporcionar el parámetro 'userId' o el término de búsqueda 'q'." }));
        }

        // 3. Buscamos por el término (Nombre o DNI)
        var candidatos = await _usuarioService.BuscarParaAgenteAsync(q.Trim());

        if (!candidatos.Any())
        {
            return (null, NotFound(new { error = $"No se encontró ningún usuario con el término '{q}'." }));
        }

        // 4. Si hay más de uno, intentamos buscar coincidencia EXACTA (para evitar ambigüedad innecesaria)
        if (candidatos.Count() > 1)
        {
            // Primero intentamos por DNI exacto
            var porDni = candidatos.FirstOrDefault(u => u.DNI == q.Trim());
            if (porDni != null) return (porDni.Id, null);

            // Luego por Nombre Completo exacto
            var porNombre = candidatos.FirstOrDefault(u => u.NombreCompleto.Equals(q.Trim(), StringComparison.OrdinalIgnoreCase));
            if (porNombre != null) return (porNombre.Id, null);

            // Si sigue habiendo ambigüedad, retornamos 400 con la lista de opciones
            return (null, BadRequest(new 
            { 
                error       = "Ambigüedad detectada", 
                mensaje     = $"Se encontraron {candidatos.Count()} usuarios para '{q}'. Por favor, usa el DNI o sé más específico.",
                sugerencias = candidatos 
            }));
        }

        // 5. Solo hay uno, perfecto
        return (candidatos.First().Id, null);
    }
}
