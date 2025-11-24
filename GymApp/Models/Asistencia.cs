using System;
using System.Collections.Generic;

namespace GymApp.Models;

public partial class Asistencia
{
    public int AsistenciaId { get; set; }

    public int UserId { get; set; }

    public DateTime? FechaHora { get; set; }

    public bool AccesoPermitido { get; set; }

    public string? MotivoDenegacion { get; set; }

    public virtual Usuario User { get; set; } = null!;
}
