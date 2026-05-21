using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApp.Migrations
{
    /// <inheritdoc />
    public partial class AddNivelPeligroToPermiso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NivelPeligro",
                table: "Permisos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Membresias.Crear",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Membresias.Editar",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Membresias.Eliminar",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Membresias.Ver",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Pagos.Anular",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Pagos.Crear",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Pagos.Editar",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Pagos.Ver",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "PasesDiarios.Crear",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "PasesDiarios.Editar",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "PasesDiarios.Eliminar",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "PasesDiarios.Ver",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Usuarios.Crear",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Usuarios.Editar",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Usuarios.Eliminar",
                column: "NivelPeligro",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "PermisoId",
                keyValue: "Usuarios.Ver",
                column: "NivelPeligro",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NivelPeligro",
                table: "Permisos");
        }
    }
}
