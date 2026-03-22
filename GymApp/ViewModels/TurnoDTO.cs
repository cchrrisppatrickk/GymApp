namespace GymApp.ViewModels
{
    public class TurnoDTO
    {
        public int TurnoId { get; set; }
        public string Nombre { get; set; }
        // Recibimos las horas como string para evitar error de parsing JSON
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
        public string? Descripcion { get; set; }
    }
}
