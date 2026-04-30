namespace GymApp.ViewModels
{
    public class PagoRecienteDTO
    {
        public string NombreCliente { get; set; } = string.Empty;
        public string NombrePlan { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
    }
}
