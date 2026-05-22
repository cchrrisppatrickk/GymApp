using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApp.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarPermisosDashboardYRemoverPasesDiariosEditar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "PasesDiarios.Editar");

            migrationBuilder.InsertData(
                table: "Permisos",
                columns: new[] { "PermisoId", "Descripcion", "Modulo", "NivelPeligro" },
                values: new object[] { "Dashboard.Ver", "Visualizar métricas y gráficos del dashboard", "Dashboard", 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Dashboard.Ver");

            migrationBuilder.InsertData(
                table: "Permisos",
                columns: new[] { "PermisoId", "Descripcion", "Modulo", "NivelPeligro" },
                values: new object[] { "PasesDiarios.Editar", "Editar pases diarios", "PasesDiarios", 0 });
        }
    }
}
