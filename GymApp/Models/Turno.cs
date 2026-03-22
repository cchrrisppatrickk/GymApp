using System;
using System.Collections.Generic;

namespace GymApp.Models;

public partial class Turno
{
    public int TurnoId { get; set; }

    public string Nombre { get; set; } = null!;

    public TimeSpan HoraInicio { get; set; }

    public TimeSpan HoraFin { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Membresia> Membresia { get; set; } = new List<Membresia>();
}
