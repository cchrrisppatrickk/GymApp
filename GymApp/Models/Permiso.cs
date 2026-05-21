using System.Collections.Generic;

namespace GymApp.Models
{
    public class Permiso
    {
        public string PermisoId { get; set; } = null!;
        public string Modulo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;

        public virtual ICollection<UsuarioPermiso> UsuarioPermisos { get; set; } = new List<UsuarioPermiso>();
    }
}
