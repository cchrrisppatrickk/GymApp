using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;


namespace GymApp.ViewModels
{
    public class UsuarioViewModel
    {
        public IFormFile? FotoArchivo { get; set; }
        public string? FotoBase64 { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "El Rol es obligatorio")]
        public int RoleId { get; set; }
        public string? NombreRol { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        public string NombreCompleto { get; set; }

        public string? Dni { get; set; }

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

        // Nuevos campos CRM
        public string? Origen { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? EstadoCivil { get; set; }
        public string? Genero { get; set; }
        public string? Direccion { get; set; }
        public string? WhatsApp { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public string? Ocupacion { get; set; }
        public string? Nota { get; set; }
        public string? PinAcceso { get; set; }

        public string[]? PermisosSeleccionados { get; set; }
    }
}