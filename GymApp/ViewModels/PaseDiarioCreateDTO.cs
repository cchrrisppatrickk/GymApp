using System.ComponentModel.DataAnnotations;

namespace GymApp.ViewModels;

public class PaseDiarioCreateDTO
{
    public int? UserId { get; set; }

    [Required(ErrorMessage = "El turno es requerido")]
    public int TurnoId { get; set; }

    [Required(ErrorMessage = "El monto es requerido")]
    public decimal Monto { get; set; } = 7.00m;

    [Required(ErrorMessage = "El método de pago es requerido")]
    [StringLength(50)]
    public string MetodoPago { get; set; } = null!;

    [StringLength(255)]
    public string? Observacion { get; set; }

    public string? ComprobanteBase64 { get; set; }

    public IFormFile? ComprobanteArchivo { get; set; }
}
