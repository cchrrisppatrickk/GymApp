using System;
using System.Collections.Generic;

namespace GymApp.Models;

public partial class Membresia
{
    public int MembresiaId { get; set; }

    public int UserId { get; set; }

    public int PlanId { get; set; }

    public int TurnoId { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public string? Estado { get; set; }

    public string? Observaciones { get; set; }

    public virtual ICollection<Congelamiento> Congelamientos { get; set; } = new List<Congelamiento>();

    public virtual ICollection<PagosMembresium> PagosMembresia { get; set; } = new List<PagosMembresium>();

    public virtual Plane Plan { get; set; } = null!;

    public virtual Turno Turno { get; set; } = null!;

    public virtual Usuario User { get; set; } = null!;
}
