namespace GymApp.Models
{
    public class UsuarioPermiso
    {
        public int UserId { get; set; }
        public string PermisoId { get; set; } = null!;

        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Permiso Permiso { get; set; } = null!;
    }
}
