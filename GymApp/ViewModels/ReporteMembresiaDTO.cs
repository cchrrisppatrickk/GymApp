using System;

namespace GymApp.ViewModels
{
    public class ReporteMembresiaDTO
    {
        public int MembresiaId { get; set; }
        public string NombreCliente { get; set; }
        public string Telefono { get; set; }

        // Fechas formateadas como string para facilitar la vista
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }

        public string NombrePlan { get; set; }
        public string NombreTurno { get; set; }

        // --- PAGOS SEPARADOS ---
        public decimal PagadoEfectivo { get; set; }
        public decimal PagadoYape { get; set; }

        public string Observaciones { get; set; }
        public string Estado { get; set; } // Activa, Vencida, etc.
    }
}