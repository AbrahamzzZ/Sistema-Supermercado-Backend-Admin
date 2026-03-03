using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CATEGORIA",
                columns: table => new
                {
                    ID_CATEGORIA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CODIGO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    NOMBRE_CATEGORIA = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ESTADO = table.Column<bool>(type: "bit", nullable: true),
                    FECHA_CREACION = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORIA", x => x.ID_CATEGORIA);
                });

            migrationBuilder.CreateTable(
                name: "CLIENTE",
                columns: table => new
                {
                    ID_CLIENTE = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CODIGO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    NOMBRES = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    APELLIDOS = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    CEDULA = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    TELEFONO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    CORREO_ELECTRONICO = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FECHA_REGISTRO = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLIENTE", x => x.ID_CLIENTE);
                });

            migrationBuilder.CreateTable(
                name: "NEGOCIO",
                columns: table => new
                {
                    ID_NEGOCIO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOMBRE = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    TELEFONO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    RUC = table.Column<string>(type: "varchar(13)", unicode: false, maxLength: 13, nullable: true),
                    DIRECCION = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    CORREO_ELECTRONICO = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    LOGO = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NEGOCIO", x => x.ID_NEGOCIO);
                });

            migrationBuilder.CreateTable(
                name: "PROVEEDOR",
                columns: table => new
                {
                    ID_PROVEEDOR = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CODIGO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    NOMBRES = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    APELLIDOS = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    CEDULA = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    TELEFONO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    CORREO_ELECTRONICO = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ESTADO = table.Column<bool>(type: "bit", nullable: true),
                    FECHA_REGISTRO = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROVEEDOR", x => x.ID_PROVEEDOR);
                });

            migrationBuilder.CreateTable(
                name: "TRANSPORTISTA",
                columns: table => new
                {
                    ID_TRANSPORTISTA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CODIGO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    NOMBRES = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    APELLIDOS = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    CEDULA = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    TELEFONO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    CORREO_ELECTRONICO = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IMAGEN = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ESTADO = table.Column<bool>(type: "bit", nullable: true),
                    FECHA_REGISTRO = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSPORTISTA", x => x.ID_TRANSPORTISTA);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCTO",
                columns: table => new
                {
                    ID_PRODUCTO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CODIGO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    DESCRIPCION = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NOMBRE_PRODUCTO = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ID_CATEGORIA = table.Column<int>(type: "int", nullable: true),
                    PAIS_ORIGEN = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    ESTADO = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCTO", x => x.ID_PRODUCTO);
                    table.ForeignKey(
                        name: "FK_PRODUCTO_CATEGORIA_ID_CATEGORIA",
                        column: x => x.ID_CATEGORIA,
                        principalTable: "CATEGORIA",
                        principalColumn: "ID_CATEGORIA",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SUCURSAL",
                columns: table => new
                {
                    ID_SUCURSAL = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CODIGO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    ID_NEGOCIO = table.Column<int>(type: "int", nullable: true),
                    NOMBRE_SUCURSAL = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    DIRECCION_SUCURSAL = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    Latitud = table.Column<double>(type: "float", nullable: true),
                    Longitud = table.Column<double>(type: "float", nullable: true),
                    CIUDAD_SUCURSAL = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    ESTADO = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUCURSAL", x => x.ID_SUCURSAL);
                    table.ForeignKey(
                        name: "FK_SUCURSAL_NEGOCIO_ID_NEGOCIO",
                        column: x => x.ID_NEGOCIO,
                        principalTable: "NEGOCIO",
                        principalColumn: "ID_NEGOCIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OFERTA",
                columns: table => new
                {
                    ID_OFERTA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CODIGO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    NOMBRE_OFERTA = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ID_PRODUCTO = table.Column<int>(type: "int", nullable: true),
                    DESCRIPCION = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    FECHA_INICIO = table.Column<DateOnly>(type: "date", nullable: true),
                    FECHA_FIN = table.Column<DateOnly>(type: "date", nullable: true),
                    DESCUENTO = table.Column<decimal>(type: "decimal(5,2)", nullable: true, defaultValue: 0m),
                    ESTADO = table.Column<bool>(type: "bit", nullable: true),
                    FECHA_CREACION = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OFERTA", x => x.ID_OFERTA);
                    table.ForeignKey(
                        name: "FK_OFERTA_PRODUCTO_ID_PRODUCTO",
                        column: x => x.ID_PRODUCTO,
                        principalTable: "PRODUCTO",
                        principalColumn: "ID_PRODUCTO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ__NEGOCIO__CAF3326B49023C6A",
                table: "NEGOCIO",
                column: "RUC",
                unique: true,
                filter: "[RUC] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__NEGOCIO__D6F1694583FA83D5",
                table: "NEGOCIO",
                column: "TELEFONO",
                unique: true,
                filter: "[TELEFONO] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OFERTA_ID_PRODUCTO",
                table: "OFERTA",
                column: "ID_PRODUCTO");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTO_ID_CATEGORIA",
                table: "PRODUCTO",
                column: "ID_CATEGORIA");

            migrationBuilder.CreateIndex(
                name: "IX_SUCURSAL_ID_NEGOCIO",
                table: "SUCURSAL",
                column: "ID_NEGOCIO");

            migrationBuilder.CreateIndex(
                name: "UQ__SUCURSAL__CC87E126E64D406E",
                table: "SUCURSAL",
                column: "CODIGO",
                unique: true,
                filter: "[CODIGO] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CLIENTE");

            migrationBuilder.DropTable(
                name: "OFERTA");

            migrationBuilder.DropTable(
                name: "PROVEEDOR");

            migrationBuilder.DropTable(
                name: "SUCURSAL");

            migrationBuilder.DropTable(
                name: "TRANSPORTISTA");

            migrationBuilder.DropTable(
                name: "PRODUCTO");

            migrationBuilder.DropTable(
                name: "NEGOCIO");

            migrationBuilder.DropTable(
                name: "CATEGORIA");
        }
    }
}
