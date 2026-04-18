namespace GymApp.ViewModels
{
    public class DashboardUserStatsDTO
    {
        public int NuevosMiembrosMes { get; set; }
        public int VencidosSinRenovar { get; set; }
        public int PorVencer7Dias { get; set; }
        public int UsuariosConDeuda { get; set; }
        public decimal MontoTotalDeuda { get; set; }
        public int MembresiasCongeladas { get; set; }
    }
}
