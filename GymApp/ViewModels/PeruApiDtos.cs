namespace GymApp.ViewModels
{
    public class PeruApiDniResponse
    {
        public string? Dni { get; set; }
        public string? Cliente { get; set; } // Nombre completo
        public string? Nombres { get; set; }
        public string? Apellido_paterno { get; set; }
        public string? Apellido_materno { get; set; }
        public string? Mensaje { get; set; }
        public string? Code { get; set; }
    }
}