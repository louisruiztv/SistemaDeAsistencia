using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeAsistencia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGeolocalizacionToAsistencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Longitud",
                table: "Asistencias",
                newName: "LongitudSalida");

            migrationBuilder.RenameColumn(
                name: "Latitud",
                table: "Asistencias",
                newName: "LongitudEntrada");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Entrada",
                table: "Asistencias",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha",
                table: "Asistencias",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "LatitudEntrada",
                table: "Asistencias",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LatitudSalida",
                table: "Asistencias",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fecha",
                table: "Asistencias");

            migrationBuilder.DropColumn(
                name: "LatitudEntrada",
                table: "Asistencias");

            migrationBuilder.DropColumn(
                name: "LatitudSalida",
                table: "Asistencias");

            migrationBuilder.RenameColumn(
                name: "LongitudSalida",
                table: "Asistencias",
                newName: "Longitud");

            migrationBuilder.RenameColumn(
                name: "LongitudEntrada",
                table: "Asistencias",
                newName: "Latitud");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Entrada",
                table: "Asistencias",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
