using System;
using System.Collections.Generic;

namespace GymApp.Models;

public partial class VentasCabecera
{
    public int VentaId { get; set; }

    public int? UserId { get; set; }

    public int UsuarioEmpleadoId { get; set; }

    public DateTime? FechaVenta { get; set; }

    public decimal Total { get; set; }

    public string MetodoPago { get; set; } = null!;

    public virtual Usuario? User { get; set; }

    public virtual Usuario UsuarioEmpleado { get; set; } = null!;

    public virtual ICollection<VentasDetalle> VentasDetalles { get; set; } = new List<VentasDetalle>();
}
