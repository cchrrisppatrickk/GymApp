using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Models;

public partial class PaseDiario
{
    public int PaseDiarioId { get; set; }

    public int? UserId { get; set; }

    public int TurnoId { get; set; }

    public int UsuarioEmpleadoId { get; set; }

    public decimal Monto { get; set; } = 7.00m;

    public string MetodoPago { get; set; } = null!;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public string? Observacion { get; set; }

    [StringLength(500)]
    public string? ComprobanteRuta { get; set; }

    public virtual Usuario? User { get; set; }

    public virtual Turno Turno { get; set; } = null!;

    public virtual Usuario UsuarioEmpleado { get; set; } = null!;
}
