using System;
using System.ComponentModel.DataAnnotations;

namespace GymApp.Models;

public class ConfiguracionAlerta
{
    [Key]
    public int Id { get; set; }

    public TimeSpan HoraEnvio { get; set; }

    public string DiasSemana { get; set; } = string.Empty;

    public bool EnviarNuevosMiembros { get; set; }

    public bool EnviarProximosVencimientos { get; set; }

    public bool EnviarDeudasPendientes { get; set; }

    public bool EnviarPagosHoy { get; set; }
    public bool AvisarNuevoUsuario { get; set; }
    public bool AvisarNuevoPago { get; set; }
    public bool AvisarNuevaMembresia { get; set; }

    public string ChatIdDestino { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public DateTime? UltimaEjecucionVencimientos { get; set; }
}
