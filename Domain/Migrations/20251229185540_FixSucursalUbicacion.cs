using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class FixSucursalUbicacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitud",
                table: "SUCURSAL");

            migrationBuilder.DropColumn(
                name: "Longitud",
                table: "SUCURSAL");

            migrationBuilder.AddColumn<Point>(
                name: "UBICACION_SUCURSAL",
                table: "SUCURSAL",
                type: "geography",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UBICACION_SUCURSAL",
                table: "SUCURSAL");

            migrationBuilder.AddColumn<double>(
                name: "Latitud",
                table: "SUCURSAL",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitud",
                table: "SUCURSAL",
                type: "float",
                nullable: true);
        }
    }
}
