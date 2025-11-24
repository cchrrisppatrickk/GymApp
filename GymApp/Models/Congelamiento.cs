using System;
using System.Collections.Generic;

namespace GymApp.Models;

public partial class Congelamiento
{
    public int CongelamientoId { get; set; }

    public int MembresiaId { get; set; }

    public int UsuarioEmpleadoId { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public string? Motivo { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual Membresia Membresia { get; set; } = null!;

    public virtual Usuario UsuarioEmpleado { get; set; } = null!;
}
