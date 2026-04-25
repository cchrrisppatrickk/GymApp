namespace GymApp.ViewModels
{
    // Lo que enviamos a la tabla (con nombres en lugar de IDs)
    public class MembresiaListDTO
    {
        public int MembresiaId { get; set; }
        public int UserId { get; set; }
        public string NombreUsuario { get; set; }
        public string? Dni { get; set; }
        public string NombrePlan { get; set; }
        public string NombreTurno { get; set; }
        public string FechaInicio { get; set; }
        public string FechaVencimiento { get; set; }
        public string Estado { get; set; } // Activa, Vencida, Por Vencer
        public int DiasRestantes { get; set; }
        public int DiasVencidos { get; set; }
        public bool PermiteCongelar { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal DeudaPendiente { get; set; }
        public decimal Deuda { get; set; } // Mantener por compatibilidad si es necesario, pero usaremos DeudaPendiente
    }
}
