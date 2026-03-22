using System;
using System.Collections.Generic;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Data;

public partial class GymDbContext : DbContext
{
    public GymDbContext()
    {
    }

    public GymDbContext(DbContextOptions<GymDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asistencia> Asistencias { get; set; }

    public virtual DbSet<Congelamiento> Congelamientos { get; set; }

    public virtual DbSet<Membresia> Membresias { get; set; }

    public virtual DbSet<PagosMembresium> PagosMembresia { get; set; }

    public virtual DbSet<Plane> Planes { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Turno> Turnos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VentasCabecera> VentasCabeceras { get; set; }

    public virtual DbSet<VentasDetalle> VentasDetalles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asistencia>(entity =>
        {
            entity.HasKey(e => e.AsistenciaId).HasName("PK__Asistenc__72710F45116423EA");

            entity.Property(e => e.AsistenciaId).HasColumnName("AsistenciaID");
            entity.Property(e => e.FechaHora)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MotivoDenegacion).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asistencias_Usuarios");
        });

        modelBuilder.Entity<Congelamiento>(entity =>
        {
            entity.HasKey(e => e.CongelamientoId).HasName("PK__Congelam__7EA0385F3FEBB624");

            entity.Property(e => e.CongelamientoId).HasColumnName("CongelamientoID");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MembresiaId).HasColumnName("MembresiaID");
            entity.Property(e => e.Motivo).HasMaxLength(200);
            entity.Property(e => e.UsuarioEmpleadoId).HasColumnName("UsuarioEmpleadoID");

            entity.HasOne(d => d.Membresia).WithMany(p => p.Congelamientos)
                .HasForeignKey(d => d.MembresiaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Congelamientos_Membresias");

            entity.HasOne(d => d.UsuarioEmpleado).WithMany(p => p.Congelamientos)
                .HasForeignKey(d => d.UsuarioEmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Congelamientos_Empleado");
        });

        modelBuilder.Entity<Membresia>(entity =>
        {
            entity.HasKey(e => e.MembresiaId).HasName("PK__Membresi__5AE93077CEEFAEC0");

            entity.Property(e => e.MembresiaId).HasColumnName("MembresiaID");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Activa");
            entity.Property(e => e.Observaciones).HasMaxLength(200);
            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.TurnoId).HasColumnName("TurnoID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Plan).WithMany(p => p.Membresia)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Membresias_Planes");

            entity.HasOne(d => d.Turno).WithMany(p => p.Membresia)
                .HasForeignKey(d => d.TurnoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Membresias_Turnos");

            entity.HasOne(d => d.User).WithMany(p => p.Membresia)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Membresias_Usuarios");
        });

        modelBuilder.Entity<PagosMembresium>(entity =>
        {
            entity.HasKey(e => e.PagoId).HasName("PK__PagosMem__F00B61585E3C891C");

            entity.Property(e => e.PagoId).HasColumnName("PagoID");
            entity.Property(e => e.Comprobante).HasMaxLength(50);
            entity.Property(e => e.FechaPago)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MembresiaId).HasColumnName("MembresiaID");
            entity.Property(e => e.MetodoPago).HasMaxLength(50);
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UsuarioEmpleadoId).HasColumnName("UsuarioEmpleadoID");

            entity.HasOne(d => d.Membresia).WithMany(p => p.PagosMembresia)
                .HasForeignKey(d => d.MembresiaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pagos_Membresias");

            entity.HasOne(d => d.UsuarioEmpleado).WithMany(p => p.PagosMembresia)
                .HasForeignKey(d => d.UsuarioEmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pagos_Empleado");
        });

        modelBuilder.Entity<Plane>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Planes__755C22D76140EBD8");

            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.PermiteCongelar).HasDefaultValue(false);
            entity.Property(e => e.PrecioBase).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.ProductoId).HasName("PK__Producto__A430AE831545AAF1");

            entity.Property(e => e.ProductoId).HasColumnName("ProductoID");
            entity.Property(e => e.Categoria)
                .HasMaxLength(50)
                .HasDefaultValue("General");
            entity.Property(e => e.CodigoBarras).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.PrecioVenta).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StockActual).HasDefaultValue(0);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A2A5A36BE");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Descripcion).HasMaxLength(100);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Turno>(entity =>
        {
            entity.HasKey(e => e.TurnoId).HasName("PK__Turnos__AD3E2EB4A28DB496");

            entity.Property(e => e.TurnoId).HasColumnName("TurnoID");
            entity.Property(e => e.Descripcion).HasMaxLength(100);
            entity.Property(e => e.HoraFin).HasPrecision(0);
            entity.Property(e => e.HoraInicio).HasPrecision(0);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Usuarios__1788CCAC51338C72");

            entity.HasIndex(e => e.NombreUsuario, "UQ__Usuarios__6B0F5AE03DC8E965").IsUnique();

            entity.HasIndex(e => e.Dni, "UQ__Usuarios__C035B8DD955B12A3").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CodigoQr)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CodigoQR");
            entity.Property(e => e.Dni)
                .HasMaxLength(15)
                .HasColumnName("DNI");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreCompleto).HasMaxLength(100);
            entity.Property(e => e.NombreUsuario).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.Role).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        modelBuilder.Entity<VentasCabecera>(entity =>
        {
            entity.HasKey(e => e.VentaId).HasName("PK__VentasCa__5B41514CE0132551");

            entity.ToTable("VentasCabecera");

            entity.Property(e => e.VentaId).HasColumnName("VentaID");
            entity.Property(e => e.FechaVenta)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MetodoPago).HasMaxLength(50);
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.UsuarioEmpleadoId).HasColumnName("UsuarioEmpleadoID");

            entity.HasOne(d => d.User).WithMany(p => p.VentasCabeceraUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Ventas_Cliente");

            entity.HasOne(d => d.UsuarioEmpleado).WithMany(p => p.VentasCabeceraUsuarioEmpleados)
                .HasForeignKey(d => d.UsuarioEmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Empleado");
        });

        modelBuilder.Entity<VentasDetalle>(entity =>
        {
            entity.HasKey(e => e.DetalleId).HasName("PK__VentasDe__6E19D6FA94348278");

            entity.ToTable("VentasDetalle");

            entity.Property(e => e.DetalleId).HasColumnName("DetalleID");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductoId).HasColumnName("ProductoID");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.VentaId).HasColumnName("VentaID");

            entity.HasOne(d => d.Producto).WithMany(p => p.VentasDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Detalle_Producto");

            entity.HasOne(d => d.Venta).WithMany(p => p.VentasDetalles)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Detalle_Venta");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
