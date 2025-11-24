using System;
using System.Collections.Generic;

namespace GymApp.Models;

public partial class PagosMembresium
{
    public int PagoId { get; set; }

    public int MembresiaId { get; set; }

    public int UsuarioEmpleadoId { get; set; }

    public decimal Monto { get; set; }

    public string MetodoPago { get; set; } = null!;

    public DateTime? FechaPago { get; set; }

    public string? Comprobante { get; set; }

    public virtual Membresia Membresia { get; set; } = null!;

    public virtual Usuario UsuarioEmpleado { get; set; } = null!;
}
