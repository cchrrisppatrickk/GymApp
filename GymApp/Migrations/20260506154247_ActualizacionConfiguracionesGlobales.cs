using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApp.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionConfiguracionesGlobales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AvisarNuevaMembresia",
                table: "ConfiguracionAlertas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AvisarNuevoPago",
                table: "ConfiguracionAlertas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AvisarNuevoUsuario",
                table: "ConfiguracionAlertas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvisarNuevaMembresia",
                table: "ConfiguracionAlertas");

            migrationBuilder.DropColumn(
                name: "AvisarNuevoPago",
                table: "ConfiguracionAlertas");

            migrationBuilder.DropColumn(
                name: "AvisarNuevoUsuario",
                table: "ConfiguracionAlertas");
        }
    }
}
