namespace GymApp.ViewModels
{
    public class PagoCreateDTO
    {
        public int MembresiaId { get; set; } // Para indicar qué membresía se está cobrando
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = null!; // Texto libre o select
        public DateTime? FechaPago { get; set; }

        /// <summary>
        /// Imagen del comprobante en formato Base64 (enviada desde el frontend vía JSON).
        /// Obligatoria cuando MetodoPago es "Yape/Plin".
        /// </summary>
        public string? ComprobanteBase64 { get; set; }

        /// <summary>
        /// Archivo de comprobante (para flujos multipart futuros, ej. API REST externa).
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public Microsoft.AspNetCore.Http.IFormFile? ComprobanteArchivo { get; set; }
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
        public bool EsAnulado { get; set; }
    }

    // Para mostrar info previa al cobro
    public class DeudaInfoDTO
    {
        public int MembresiaId { get; set; }
        public string NombreCliente { get; set; }
        public string? DniCliente { get; set; }
        public string NombrePlan { get; set; }
        public string Estado { get; set; }

        // NUEVOS CAMPOS FINANCIEROS
        public decimal PrecioTotal { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal DeudaPendiente { get; set; } // PrecioTotal - TotalPagado
    }
}