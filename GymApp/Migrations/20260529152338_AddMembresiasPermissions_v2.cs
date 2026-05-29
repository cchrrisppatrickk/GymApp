using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMembresiasPermissions_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Membresias.Crear",
                column: "Descripcion",
                value: "Crear membresías");

            migrationBuilder.InsertData(
                table: "Permisos",
                columns: new[] { "PermisoId", "Descripcion", "Modulo", "NivelPeligro" },
                values: new object[,]
                {
                    { "Membresias.Congelar", "Congelar membresía", "Membresias", 0 },
                    { "Membresias.Renovar", "Renovar membresías", "Membresias", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Membresias.Congelar");

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Membresias.Renovar");

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Membresias.Crear",
                column: "Descripcion",
                value: "Crear/Renovar membresías");
        }
    }
}
