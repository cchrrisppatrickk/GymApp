namespace GymApp.ViewModels
{
    public class PagoEditDTO
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = null!;
        public string? Observaciones { get; set; }
    }
}
