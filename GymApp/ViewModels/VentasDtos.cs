namespace GymApp.ViewModels
{
    // Detalle simple del producto
    public class VentaItemDTO
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    // El objeto principal que recibe el controlador
    public class VentaCreateDTO
    {
        public int? UserId { get; set; } // Puede ser null si es venta a público general
        public string MetodoPago { get; set; } // "Efectivo", "Yape", etc.
        public List<VentaItemDTO> Items { get; set; }
    }
}