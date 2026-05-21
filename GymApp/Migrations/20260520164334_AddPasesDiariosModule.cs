using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPasesDiariosModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasesDiarios",
                columns: table => new
                {
                    PaseDiarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    TurnoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioEmpleadoId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Observacion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasesDiarios", x => x.PaseDiarioId);
                    table.ForeignKey(
                        name: "FK_PasesDiarios_Empleado",
                        column: x => x.UsuarioEmpleadoId,
                        principalTable: "Usuarios",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PasesDiarios_Turnos",
                        column: x => x.TurnoId,
                        principalTable: "Turnos",
                        principalColumn: "TurnoID");
                    table.ForeignKey(
                        name: "FK_PasesDiarios_Usuarios",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasesDiarios_TurnoId",
                table: "PasesDiarios",
                column: "TurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_PasesDiarios_UserId",
                table: "PasesDiarios",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasesDiarios_UsuarioEmpleadoId",
                table: "PasesDiarios",
                column: "UsuarioEmpleadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasesDiarios");
        }
    }
}
