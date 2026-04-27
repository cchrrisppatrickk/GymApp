using System;

namespace GymApp.ViewModels
{
    public class MembresiaEditDTO
    {
        public int MembresiaId { get; set; }
        public int PlanId { get; set; }
        public int TurnoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal? PrecioAcordadoPersonalizado { get; set; }
        public string? Observaciones { get; set; }
    }
}
