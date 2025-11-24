using System;
using System.Collections.Generic;

namespace GymApp.Models;

public partial class Producto
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal PrecioVenta { get; set; }

    public int? StockActual { get; set; }

    public string? CodigoBarras { get; set; }

    public virtual ICollection<VentasDetalle> VentasDetalles { get; set; } = new List<VentasDetalle>();
}
