using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPrecioAcordadoMembresias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioAcordado",
                table: "Membresias",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                @"UPDATE m 
                  SET m.PrecioAcordado = p.PrecioBase 
                  FROM Membresias m 
                  INNER JOIN Planes p ON m.PlanID = p.PlanID;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioAcordado",
                table: "Membresias");
        }
    }
}
