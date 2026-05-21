using System.Collections.Generic;

namespace GymApp.Models
{
    /// <summary>Nivel de peligro para colorear la UI de asignación de permisos.</summary>
    public enum NivelPeligro { Bajo, Medio, Alto }

    public class Permiso
    {
        public string PermisoId { get; set; } = null!;
        public string Modulo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;

        /// <summary>Clasifica el riesgo de la acción: Bajo (lectura), Medio (escritura), Alto (eliminación/anulación).</summary>
        public NivelPeligro NivelPeligro { get; set; } = NivelPeligro.Bajo;

        public virtual ICollection<UsuarioPermiso> UsuarioPermisos { get; set; } = new List<UsuarioPermiso>();
    }
}
