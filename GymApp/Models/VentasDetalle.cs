using System;
using System.Collections.Generic;

namespace GymApp.Models;

public partial class VentasDetalle
{
    public int DetalleId { get; set; }

    public int VentaId { get; set; }

    public int ProductoId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Producto Producto { get; set; } = null!;

    public virtual VentasCabecera Venta { get; set; } = null!;
}
