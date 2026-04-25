using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApp.Migrations
{
    /// <inheritdoc />
    public partial class InicializarBaseDeDatos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Planes",
                columns: table => new
                {
                    PlanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DuracionDias = table.Column<int>(type: "int", nullable: false),
                    PrecioBase = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PermiteCongelar = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Planes__755C22D76140EBD8", x => x.PlanID);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    ProductoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StockActual = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CodigoBarras = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "General")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Producto__A430AE831545AAF1", x => x.ProductoID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__8AFACE3A2A5A36BE", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Turnos",
                columns: table => new
                {
                    TurnoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time(0)", precision: 0, nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time(0)", precision: 0, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Turnos__AD3E2EB4A28DB496", x => x.TurnoID);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DNI = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CodigoQR = table.Column<Guid>(type: "uniqueidentifier", nullable: true, defaultValueSql: "(newid())"),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Estado = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuarios__1788CCAC51338C72", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID");
                });

            migrationBuilder.CreateTable(
                name: "Asistencias",
                columns: table => new
                {
                    AsistenciaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    AccesoPermitido = table.Column<bool>(type: "bit", nullable: false),
                    MotivoDenegacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Asistenc__72710F45116423EA", x => x.AsistenciaID);
                    table.ForeignKey(
                        name: "FK_Asistencias_Usuarios",
                        column: x => x.UserID,
                        principalTable: "Usuarios",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Membresias",
                columns: table => new
                {
                    MembresiaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PlanID = table.Column<int>(type: "int", nullable: false),
                    TurnoID = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Activa"),
                    Observaciones = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Membresi__5AE93077CEEFAEC0", x => x.MembresiaID);
                    table.ForeignKey(
                        name: "FK_Membresias_Planes",
                        column: x => x.PlanID,
                        principalTable: "Planes",
                        principalColumn: "PlanID");
                    table.ForeignKey(
                        name: "FK_Membresias_Turnos",
                        column: x => x.TurnoID,
                        principalTable: "Turnos",
                        principalColumn: "TurnoID");
                    table.ForeignKey(
                        name: "FK_Membresias_Usuarios",
                        column: x => x.UserID,
                        principalTable: "Usuarios",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "VentasCabecera",
                columns: table => new
                {
                    VentaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    UsuarioEmpleadoID = table.Column<int>(type: "int", nullable: false),
                    FechaVenta = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VentasCa__5B41514CE0132551", x => x.VentaID);
                    table.ForeignKey(
                        name: "FK_Ventas_Cliente",
                        column: x => x.UserID,
                        principalTable: "Usuarios",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Ventas_Empleado",
                        column: x => x.UsuarioEmpleadoID,
                        principalTable: "Usuarios",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Congelamientos",
                columns: table => new
                {
                    CongelamientoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MembresiaID = table.Column<int>(type: "int", nullable: false),
                    UsuarioEmpleadoID = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Congelam__7EA0385F3FEBB624", x => x.CongelamientoID);
                    table.ForeignKey(
                        name: "FK_Congelamientos_Empleado",
                        column: x => x.UsuarioEmpleadoID,
                        principalTable: "Usuarios",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Congelamientos_Membresias",
                        column: x => x.MembresiaID,
                        principalTable: "Membresias",
                        principalColumn: "MembresiaID");
                });

            migrationBuilder.CreateTable(
                name: "PagosMembresia",
                columns: table => new
                {
                    PagoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MembresiaID = table.Column<int>(type: "int", nullable: false),
                    UsuarioEmpleadoID = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Comprobante = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PagosMem__F00B61585E3C891C", x => x.PagoID);
                    table.ForeignKey(
                        name: "FK_Pagos_Empleado",
                        column: x => x.UsuarioEmpleadoID,
                        principalTable: "Usuarios",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Pagos_Membresias",
                        column: x => x.MembresiaID,
                        principalTable: "Membresias",
                        principalColumn: "MembresiaID");
                });

            migrationBuilder.CreateTable(
                name: "VentasDetalle",
                columns: table => new
                {
                    DetalleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaID = table.Column<int>(type: "int", nullable: false),
                    ProductoID = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VentasDe__6E19D6FA94348278", x => x.DetalleID);
                    table.ForeignKey(
                        name: "FK_Detalle_Producto",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID");
                    table.ForeignKey(
                        name: "FK_Detalle_Venta",
                        column: x => x.VentaID,
                        principalTable: "VentasCabecera",
                        principalColumn: "VentaID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_UserID",
                table: "Asistencias",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Congelamientos_MembresiaID",
                table: "Congelamientos",
                column: "MembresiaID");

            migrationBuilder.CreateIndex(
                name: "IX_Congelamientos_UsuarioEmpleadoID",
                table: "Congelamientos",
                column: "UsuarioEmpleadoID");

            migrationBuilder.CreateIndex(
                name: "IX_Membresias_PlanID",
                table: "Membresias",
                column: "PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_Membresias_TurnoID",
                table: "Membresias",
                column: "TurnoID");

            migrationBuilder.CreateIndex(
                name: "IX_Membresias_UserID",
                table: "Membresias",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PagosMembresia_MembresiaID",
                table: "PagosMembresia",
                column: "MembresiaID");

            migrationBuilder.CreateIndex(
                name: "IX_PagosMembresia_UsuarioEmpleadoID",
                table: "PagosMembresia",
                column: "UsuarioEmpleadoID");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RoleID",
                table: "Usuarios",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "UQ__Usuarios__6B0F5AE03DC8E965",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Usuarios__C035B8DD955B12A3",
                table: "Usuarios",
                column: "DNI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentasCabecera_UserID",
                table: "VentasCabecera",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_VentasCabecera_UsuarioEmpleadoID",
                table: "VentasCabecera",
                column: "UsuarioEmpleadoID");

            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalle_ProductoID",
                table: "VentasDetalle",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalle_VentaID",
                table: "VentasDetalle",
                column: "VentaID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asistencias");

            migrationBuilder.DropTable(
                name: "Congelamientos");

            migrationBuilder.DropTable(
                name: "PagosMembresia");

            migrationBuilder.DropTable(
                name: "VentasDetalle");

            migrationBuilder.DropTable(
                name: "Membresias");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "VentasCabecera");

            migrationBuilder.DropTable(
                name: "Planes");

            migrationBuilder.DropTable(
                name: "Turnos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
