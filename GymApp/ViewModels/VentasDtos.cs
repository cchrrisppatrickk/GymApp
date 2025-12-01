using System.Collections.Generic;

namespace GymApp.ViewModels
{
    public class VentaItemDTO
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }

        // NUEVO: Recibe el precio editado del carrito. 
        // Si no se editó, el frontend puede mandar 0 o el precio original.
        public decimal PrecioUnitario { get; set; }
    }

    public class VentaCreateDTO
    {
        public int? UserId { get; set; }
        public string MetodoPago { get; set; }
        public List<VentaItemDTO> Items { get; set; }
    }
}