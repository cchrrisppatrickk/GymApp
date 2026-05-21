using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPermisosSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    PermisoId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.PermisoId);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioPermisos",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PermisoId = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioPermisos", x => new { x.UserId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_UsuarioPermisos_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permisos",
                        principalColumn: "PermisoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioPermisos_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permisos",
                columns: new[] { "PermisoId", "Descripcion", "Modulo" },
                values: new object[,]
                {
                    { "Membresias.Crear", "Crear/Renovar membresías", "Membresias" },
                    { "Membresias.Editar", "Editar detalles de membresías", "Membresias" },
                    { "Membresias.Eliminar", "Eliminar membresías físicamente", "Membresias" },
                    { "Membresias.Ver", "Ver listado y detalles de membresías", "Membresias" },
                    { "Pagos.Anular", "Anular pagos y recalcular deudas", "Pagos" },
                    { "Pagos.Crear", "Registrar nuevos pagos", "Pagos" },
                    { "Pagos.Editar", "Editar detalles de pagos", "Pagos" },
                    { "Pagos.Ver", "Ver listado y detalles de pagos", "Pagos" },
                    { "PasesDiarios.Crear", "Crear pases diarios", "PasesDiarios" },
                    { "PasesDiarios.Editar", "Editar pases diarios", "PasesDiarios" },
                    { "PasesDiarios.Eliminar", "Eliminar pases diarios", "PasesDiarios" },
                    { "PasesDiarios.Ver", "Ver listado de pases diarios", "PasesDiarios" },
                    { "Usuarios.Crear", "Crear nuevos usuarios", "Usuarios" },
                    { "Usuarios.Editar", "Editar usuarios existentes", "Usuarios" },
                    { "Usuarios.Eliminar", "Eliminar usuarios", "Usuarios" },
                    { "Usuarios.Ver", "Ver listado y detalles de usuarios", "Usuarios" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPermisos_PermisoId",
                table: "UsuarioPermisos",
                column: "PermisoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuarioPermisos");

            migrationBuilder.DropTable(
                name: "Permisos");
        }
    }
}
