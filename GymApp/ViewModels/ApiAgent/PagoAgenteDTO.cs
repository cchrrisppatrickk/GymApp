namespace GymApp.ViewModels.ApiAgent;

public class PagoAgenteDTO
{
    public int Id { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public string MetodoPago { get; set; }
    public string NombreCliente { get; set; } // Útil para búsquedas por rango de fechas
}
