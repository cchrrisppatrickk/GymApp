using GymApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public interface IReporteService
    {
        Task<List<ReporteIngresosDTO>> ObtenerReporteMensualAsync(int mes, int anio);
        Task<List<ReporteMembresiaDTO>> ObtenerReporteMembresiasAsync(int mes, int anio);
    }
}