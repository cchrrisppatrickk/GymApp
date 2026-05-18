namespace GymApp.ViewModels;

/// <summary>
/// Tres listas paralelas para alimentar gráficos de tendencia financiero/matrículas.
/// Índice N de Etiquetas corresponde al índice N de Ingresos y Matriculas.
/// </summary>
public class GraficoTendenciaDTO
{
    /// <summary>Etiquetas del eje X (días "dd/MM" o meses "MMM yyyy").</summary>
    public List<string> Etiquetas { get; set; } = new();

    /// <summary>Ingresos totales (pagos no anulados) por período.</summary>
    public List<decimal> Ingresos { get; set; } = new();

    /// <summary>Cantidad de membresías creadas/renovadas por período.</summary>
    public List<int> Matriculas { get; set; } = new();
}
