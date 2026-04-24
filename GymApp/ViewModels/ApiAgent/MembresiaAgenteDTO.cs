namespace GymApp.ViewModels.ApiAgent;

/// <summary>
/// DTO ultra-ligero para consultas de membresías desde el agente IA (n8n).
/// Contiene sólo los campos operativos necesarios para que el LLM pueda tomar
/// decisiones sobre el estado y alertas de membresías.
/// Excluye intencionalmente: PlanId, TurnoId, UserId, y toda relación de navegación.
/// </summary>
public class MembresiaAgenteDTO
{
    public int      Id              { get; set; }
    public string   NombrePlan      { get; set; }

    /// <summary>Estado real calculado: "Activa", "Vencida", "Congelada", "Pendiente Pago".</summary>
    public string   Estado          { get; set; }

    public DateTime FechaInicio     { get; set; }
    public DateTime FechaFin        { get; set; }

    /// <summary>
    /// Días que faltan para vencer. Negativo = días de atraso desde el vencimiento.
    /// </summary>
    public int      DiasRestantes   { get; set; }

    public string   Comentarios     { get; set; }

    /// <summary>
    /// Monto pendiente de pago = PrecioAcordado - Σ(Pagos).
    /// 0 = completamente pagada. Positivo = hay deuda.
    /// </summary>
    public decimal  DeudaPendiente  { get; set; }
}
