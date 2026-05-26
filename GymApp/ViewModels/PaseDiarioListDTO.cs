using System;

namespace GymApp.ViewModels;

public class PaseDiarioListDTO
{
    public int PaseDiarioId { get; set; }
    public string NombreCliente { get; set; } = null!;
    public string NombreTurno { get; set; } = null!;
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = null!;
    public string NombreEmpleado { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string? Observacion { get; set; }
    public string? ComprobanteRuta { get; set; }
}
