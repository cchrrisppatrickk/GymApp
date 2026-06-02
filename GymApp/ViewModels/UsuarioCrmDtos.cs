using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GymApp.ViewModels
{
    public class UsuarioCreateDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string NombreCompleto { get; set; } = null!;

        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "El Rol es obligatorio")]
        public int RoleId { get; set; }

        public string? Dni { get; set; }
        public string? Email { get; set; }
        public string? WhatsApp { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        public DateOnly? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? EstadoCivil { get; set; }
        public string? Origen { get; set; }

        public string? Ocupacion { get; set; }
        public string? Nota { get; set; }
        public string? PinAcceso { get; set; }

        public string? NombreUsuario { get; set; }
        public string? Password { get; set; }

        public IFormFile? FotoArchivo { get; set; }
    }

    public class UsuarioEditDTO
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string NombreCompleto { get; set; } = null!;

        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "El Rol es obligatorio")]
        public int RoleId { get; set; }

        public string? Dni { get; set; }
        public string? Email { get; set; }
        public string? WhatsApp { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        public DateOnly? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? EstadoCivil { get; set; }
        public string? Origen { get; set; }

        public string? Ocupacion { get; set; }
        public string? Nota { get; set; }
        public string? PinAcceso { get; set; }

        public bool Estado { get; set; }

        public string? NombreUsuario { get; set; }
        public string? Password { get; set; }

        public IFormFile? FotoArchivo { get; set; }
        
        // Auditoría (Solo lectura para la vista)
        public DateTime? FechaUltimaModificacion { get; set; }
        public string? ModificadoPorNombre { get; set; }
    }

    public class UsuarioDetailsDTO
    {
        public int UserId { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? NombreRol { get; set; }
        public string? Dni { get; set; }
        public string? Email { get; set; }
        public string? WhatsApp { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? FotoUrl { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? EstadoCivil { get; set; }
        public string? Origen { get; set; }
        public string? Ocupacion { get; set; }
        public string? Nota { get; set; }
        public string? PinAcceso { get; set; }
        public bool Estado { get; set; }
        public DateTime? FechaRegistro { get; set; }
        
        public DateTime? FechaUltimaModificacion { get; set; }
        public string? ModificadoPorNombre { get; set; }

        public List<RestriccionDTO> Restricciones { get; set; } = new();
        public List<MembresiaListDTO> Membresias { get; set; } = new();

        // Fidelización
        public int TotalAsistencias { get; set; }
        public int DiasTranscurridos { get; set; }
        public decimal PorcentajeEfectividad { get; set; }
        public string NivelFidelidad { get; set; } = "N/A";
    }

    public class RestriccionDTO
    {
        public int Id { get; set; }
        public string TipoRestriccion { get; set; } = null!;
        public string? Descripcion { get; set; }
        public DateTime FechaAplicacion { get; set; }
        public string? UsuarioAplicadorNombre { get; set; }
        public bool EstadoActiva { get; set; }
    }
}
