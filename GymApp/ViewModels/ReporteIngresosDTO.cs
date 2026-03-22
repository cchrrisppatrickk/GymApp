namespace GymApp.ViewModels
{
    public class ReporteIngresosDTO
    {
        public int Dia { get; set; }

        // --- BEBIDAS ---
        public decimal BebidasMananaEfectivo { get; set; }
        public decimal BebidasMananaYape { get; set; }
        public decimal BebidasTardeEfectivo { get; set; }
        public decimal BebidasTardeYape { get; set; }
        public decimal TotalBebidas => BebidasMananaEfectivo + BebidasMananaYape + BebidasTardeEfectivo + BebidasTardeYape;

        // --- LIBRES ---
        public decimal LibresMananaEfectivo { get; set; }
        public decimal LibresMananaYape { get; set; }
        public decimal LibresTardeEfectivo { get; set; }
        public decimal LibresTardeYape { get; set; }
        public decimal TotalLibres => LibresMananaEfectivo + LibresMananaYape + LibresTardeEfectivo + LibresTardeYape;

        // --- XB ---
        public decimal XBMananaEfectivo { get; set; }
        public decimal XBMananaYape { get; set; }
        public decimal XBTardeEfectivo { get; set; }
        public decimal XBTardeYape { get; set; }
        public decimal TotalXB => XBMananaEfectivo + XBMananaYape + XBTardeEfectivo + XBTardeYape;

        // ==========================================
        // NUEVAS PROPIEDADES AGREGADAS
        // ==========================================

        // Suma de todo el efectivo del día (Bebidas + Libres + XB)
        public decimal TotalDiaEfectivo =>
            BebidasMananaEfectivo + BebidasTardeEfectivo +
            LibresMananaEfectivo + LibresTardeEfectivo +
            XBMananaEfectivo + XBTardeEfectivo;

        // Suma de todo el Yape del día
        public decimal TotalDiaYape =>
            BebidasMananaYape + BebidasTardeYape +
            LibresMananaYape + LibresTardeYape +
            XBMananaYape + XBTardeYape;

        // Total General (Suma de los dos anteriores)
        public decimal TotalGeneralDia => TotalDiaEfectivo + TotalDiaYape;
    }
}