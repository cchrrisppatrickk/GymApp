namespace GymApp.ViewModels
{
    // Lo que recibimos del formulario JS
    public class MembresiaCreateDTO
    {
        public int UserId { get; set; }
        public int PlanId { get; set; }
        public int TurnoId { get; set; }
        public DateTime FechaInicio { get; set; }
    }
}
