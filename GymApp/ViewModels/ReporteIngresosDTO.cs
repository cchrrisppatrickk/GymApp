namespace GymApp.ViewModels // O GymApp_ViewModels según tu estructura
{
    public class ReporteIngresosDTO
    {
        public int Dia { get; set; }

        // --- BEBIDAS (Categoria = 'General') ---
        public decimal BebidasMananaEfectivo { get; set; }
        public decimal BebidasMananaYape { get; set; }
        public decimal BebidasTardeEfectivo { get; set; }
        public decimal BebidasTardeYape { get; set; }
        public decimal TotalBebidas => BebidasMananaEfectivo + BebidasMananaYape + BebidasTardeEfectivo + BebidasTardeYape;

        // --- LIBRES (Categoria = 'Servicio') ---
        public decimal LibresMananaEfectivo { get; set; }
        public decimal LibresMananaYape { get; set; }
        public decimal LibresTardeEfectivo { get; set; }
        public decimal LibresTardeYape { get; set; }
        public decimal TotalLibres => LibresMananaEfectivo + LibresMananaYape + LibresTardeEfectivo + LibresTardeYape;

        // --- XB / ENERGIZANTES (Categoria = 'XB') ---
        public decimal XBMananaEfectivo { get; set; }
        public decimal XBMananaYape { get; set; }
        public decimal XBTardeEfectivo { get; set; }
        public decimal XBTardeYape { get; set; }
        public decimal TotalXB => XBMananaEfectivo + XBMananaYape + XBTardeEfectivo + XBTardeYape;

        // --- TOTAL FINAL DEL DÍA ---
        public decimal TotalGeneralDia => TotalBebidas + TotalLibres + TotalXB;
    }
}