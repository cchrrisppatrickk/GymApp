using System;

namespace GymApp.Models;

public partial class RestriccionUsuario
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string TipoRestriccion { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime FechaAplicacion { get; set; }

    public int UsuarioAplicadorId { get; set; }

    public bool EstadoActiva { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
