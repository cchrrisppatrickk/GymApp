namespace GymApp.ViewModels
{
    /// <summary>
    /// DTO de solo lectura para mostrar el detalle completo de un pago.
    /// Incluye datos del cliente, empleado que registró el cobro y la ruta
    /// física del comprobante de pago (Yape/Plin u otros).
    /// </summary>
    public class PagoDetalleDTO
    {
        // ── Datos del pago ──────────────────────────────────────────────────
        public int PagoId { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = null!;
        public DateTime? FechaPago { get; set; }

        /// <summary>
        /// Ruta relativa de la imagen del comprobante.
        /// Ejemplo: "/uploads/comprobantes/yape_3_20250518_134500.jpg"
        /// Null si el método de pago no requirió comprobante.
        /// </summary>
        public string? Comprobante { get; set; }

        public string? Observaciones { get; set; }
        public bool EsAnulado { get; set; }

        // ── Seguimiento de deuda ────────────────────────────────────────────
        /// <summary>Precio acordado total de la membresía.</summary>
        public decimal MontoTotal { get; set; }

        /// <summary>Suma de todos los pagos válidos hasta e incluyendo este pago.</summary>
        public decimal MontoPagadoAcumulado { get; set; }

        /// <summary>Deuda que quedó pendiente después de registrar este pago.</summary>
        public decimal DeudaRestante { get; set; }

        // ── Datos relacionales ──────────────────────────────────────────────
        public string NombreCliente { get; set; } = null!;
        public string? DniCliente { get; set; }
        public string NombreEmpleado { get; set; } = null!;
        public string PlanMembresia { get; set; } = null!;
        public int MembresiaId { get; set; }
    }
}
