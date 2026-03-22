using System.ComponentModel.DataAnnotations;

namespace GymApp.ViewModels
{
    public class RoleViewModel
    {
        // Usamos int? para manejar el estado de "nuevo registro" (null) vs "edición"
        public int? RoleId { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        [Display(Name = "Nombre del Rol")]
        public string Nombre { get; set; }

        [StringLength(100, ErrorMessage = "La descripción es muy larga.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        // Ventaja: Podemos agregar campos solo de lectura que no van a la BD
        // Por ejemplo, para mostrar cuántos usuarios tienen este rol en el Index
        [Display(Name = "Usuarios Activos")]
        public int CantidadUsuarios { get; set; }
    }
}