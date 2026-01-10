using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeAsistencia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregacionDeDireccionAAsistencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DireccionEntrada",
                table: "Asistencias",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DireccionSalida",
                table: "Asistencias",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DireccionEntrada",
                table: "Asistencias");

            migrationBuilder.DropColumn(
                name: "DireccionSalida",
                table: "Asistencias");
        }
    }
}
