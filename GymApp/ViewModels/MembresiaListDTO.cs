namespace GymApp.ViewModels
{
    // Lo que enviamos a la tabla (con nombres en lugar de IDs)
    public class MembresiaListDTO
    {
        public int MembresiaId { get; set; }
        public int UserId { get; set; }
        public string NombreUsuario { get; set; }
        public string Dni { get; set; }
        public string NombrePlan { get; set; }
        public string NombreTurno { get; set; }
        public string FechaInicio { get; set; }
        public string FechaVencimiento { get; set; }
        public string Estado { get; set; } // Activa, Vencida, Por Vencer
        public int DiasRestantes { get; set; }
    }
}
