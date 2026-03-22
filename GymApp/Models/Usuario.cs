using System;
using System.Collections.Generic;

namespace GymApp.Models;

public partial class Usuario
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string NombreUsuario { get; set; } = null!;

    public string Dni { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public Guid? CodigoQr { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<Asistencia> Asistencia { get; set; } = new List<Asistencia>();

    public virtual ICollection<Congelamiento> Congelamientos { get; set; } = new List<Congelamiento>();

    public virtual ICollection<Membresia> Membresia { get; set; } = new List<Membresia>();

    public virtual ICollection<PagosMembresium> PagosMembresia { get; set; } = new List<PagosMembresium>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<VentasCabecera> VentasCabeceraUsers { get; set; } = new List<VentasCabecera>();

    public virtual ICollection<VentasCabecera> VentasCabeceraUsuarioEmpleados { get; set; } = new List<VentasCabecera>();
}
