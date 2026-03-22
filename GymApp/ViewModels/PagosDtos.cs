namespace GymApp.ViewModels
{
    public class PagoCreateDTO
    {
        public string DniCliente { get; set; } // Para buscar la membresía
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } // Texto libre o select
    }

    public class PagoListDTO
    {
        public int PagoId { get; set; }
        public string NombreCliente { get; set; }
        public string NombrePlan { get; set; }
        public string MetodoPago { get; set; }
        public decimal Monto { get; set; }
        public string FechaPago { get; set; }
        public string NombreEmpleado { get; set; } // Auditoría
    }

    // Para mostrar info previa al cobro
    public class DeudaInfoDTO
    {
        public int MembresiaId { get; set; }
        public string Cliente { get; set; }
        public string Plan { get; set; }
        public string Estado { get; set; }

        // NUEVOS CAMPOS FINANCIEROS
        public decimal PrecioTotal { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal DeudaPendiente { get; set; } // PrecioTotal - TotalPagado
    }
}