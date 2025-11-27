using System.ComponentModel.DataAnnotations;

namespace GymApp.ViewModels
{
    public class UsuarioViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "El Rol es obligatorio")]
        public int RoleId { get; set; }
        public string? NombreRol { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        public string Dni { get; set; }

        // --- CAMBIO CLAVE: QUITAR [Required] ---
        // Estos campos ahora son opcionales. El controlador los llenará si vienen nulos.
        public string? NombreUsuario { get; set; }
        public string? Password { get; set; }

        // --- CAMBIO CLAVE: ELIMINAR CONFIRM PASSWORD ---
        // Si la recepcionista no pone contraseña, no tiene sentido confirmar nada.
        // Lo quitamos para evitar conflictos de validación.
        // public string? ConfirmPassword { get; set; } 

        [EmailAddress]
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public bool Estado { get; set; }
    }
}