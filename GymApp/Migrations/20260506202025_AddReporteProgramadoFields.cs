using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApp.Migrations
{
    /// <inheritdoc />
    public partial class AddReporteProgramadoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnviarResumenFinanciero",
                table: "ConfiguracionAlertas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaEjecucionReporte",
                table: "ConfiguracionAlertas",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnviarResumenFinanciero",
                table: "ConfiguracionAlertas");

            migrationBuilder.DropColumn(
                name: "UltimaEjecucionReporte",
                table: "ConfiguracionAlertas");
        }
    }
}
