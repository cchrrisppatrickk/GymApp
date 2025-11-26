using System.ComponentModel.DataAnnotations;

namespace GymApp.ViewModels
{
    public class UsuarioViewModel
    {
        public int UserId { get; set; } // 0 si es crear

        [Required(ErrorMessage = "El Rol es obligatorio")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        public string Dni { get; set; }

        public string? Email { get; set; }

        public string? Telefono { get; set; }

        // La contraseña es opcional en la edición, pero obligatoria en la creación.
        // Lo validaremos en el controlador.
        public string? Password { get; set; }

        public bool Estado { get; set; } = true;

        // Propiedad extra para mostrar el nombre del rol en la tabla sin complicaciones
        public string? NombreRol { get; set; }
    }
}