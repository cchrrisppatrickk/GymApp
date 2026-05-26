namespace GymApp.ViewModels;

/// <summary>
/// DTO para el gráfico de tendencia de Pases Diarios en el Dashboard.
/// Contiene listas paralelas para el total general y por turno (Mañana/Tarde).
/// El índice N de Etiquetas corresponde al índice N de todos los datasets.
/// </summary>
public class GraficoPaseDiarioDTO
{
    /// <summary>Etiquetas del eje X: días "dd/MM" o meses "MMM yyyy".</summary>
    public List<string> Etiquetas { get; set; } = new();

    /// <summary>Monto total de pases por período (todos los turnos).</summary>
    public List<decimal> TotalGeneral { get; set; } = new();

    /// <summary>Monto de pases del turno Mañana por período.</summary>
    public List<decimal> TurnoManana { get; set; } = new();

    /// <summary>Monto de pases del turno Tarde por período.</summary>
    public List<decimal> TurnoTarde { get; set; } = new();

    /// <summary>Cantidad total de pases del turno Mañana en el período completo.</summary>
    public int TotalPasesManana { get; set; }

    /// <summary>Cantidad total de pases del turno Tarde en el período completo.</summary>
    public int TotalPasesTarde { get; set; }

    /// <summary>Cantidad total de pases (todos los turnos) en el período completo.</summary>
    public int TotalPases => TotalPasesManana + TotalPasesTarde;
}
