using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeAsistencia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoToJustificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Justificaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Justificaciones");
        }
    }
}
