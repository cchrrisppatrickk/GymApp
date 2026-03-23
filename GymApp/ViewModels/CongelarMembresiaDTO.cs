using System;

namespace GymApp.ViewModels
{
    public class CongelarMembresiaDTO
    {
        public int MembresiaId { get; set; }
        
        // Fecha en la que inicia la pausa
        public DateTime FechaInicio { get; set; }
        
        // Fecha en la que terminaría (proyectado) o simplemente duración
        public DateTime FechaFin { get; set; }
        
        public string? Motivo { get; set; }
        
        // El ID del usuario empleado que realiza el congelamiento (de la sesión)
        public int UsuarioEmpleadoId { get; set; }
    }
}
