using System;
using System.Collections.Generic;

namespace GymApp.Models;

public partial class Plane
{
    public int PlanId { get; set; }

    public string Nombre { get; set; } = null!;

    public int DuracionDias { get; set; }

    public decimal PrecioBase { get; set; }

    public bool? PermiteCongelar { get; set; }

    public virtual ICollection<Membresia> Membresia { get; set; } = new List<Membresia>();
}
