using System;
using System.Collections.Generic;

namespace GymApp.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
