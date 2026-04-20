using System.Collections.Generic;

namespace GymApp.ViewModels
{
    public class DashboardFinancialStatsDTO
    {
        public List<string> MesesLabels { get; set; } = new();
        public List<decimal> IngresosMensuales { get; set; } = new();
        public List<string> SemanasLabels { get; set; } = new();
        public List<decimal> IngresosSemanales { get; set; } = new();
        public decimal IngresoMesActual { get; set; }
        public decimal CrecimientoMensualPorcentaje { get; set; }
        public decimal IngresoSemanaActual { get; set; }
        public decimal CrecimientoSemanalPorcentaje { get; set; }
    }
}
