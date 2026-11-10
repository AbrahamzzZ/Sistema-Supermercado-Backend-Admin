using System;
using Domain.Migrations.Scripts;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaYProcedimientos : Migration
    {
        private static readonly string[] TablasConFechaRegistro = { "CLIENTE", "PROVEEDOR", "TRANSPORTISTA" };

        // Orden de ejecución: primero los SP por módulo y al final los datos iniciales
        private static readonly string[] Scripts =
        {
            "V1_01_Negocio.sql",
            "V1_02_Sucursal.sql",
            "V1_03_Proveedor.sql",
            "V1_04_Transportista.sql",
            "V1_05_Cliente.sql",
            "V1_06_Categoria.sql",
            "V1_07_Producto.sql",
            "V1_08_Oferta.sql",
            "V1_99_DatosIniciales.sql"
        };

        private static readonly string[] Procedimientos =
        {
            "PA_EDITAR_CATEGORIA",
            "PA_EDITAR_CLIENTE",
            "PA_EDITAR_NEGOCIO",
            "PA_EDITAR_OFERTA",
            "PA_EDITAR_PRODUCTO",
            "PA_EDITAR_PROVEEDOR",
            "PA_EDITAR_SUCURSAL",
            "PA_EDITAR_TRANSPORTISTA",
            "PA_ELIMINAR_CATEGORIA",
            "PA_ELIMINAR_CLIENTE",
            "PA_ELIMINAR_OFERTA",
            "PA_ELIMINAR_PRODUCTO",
            "PA_ELIMINAR_PROVEEDOR",
            "PA_ELIMINAR_SUCURSAL",
            "PA_ELIMINAR_TRANSPORTISTA",
            "PA_LISTA_CATEGORIA",
            "PA_LISTA_CATEGORIA_PAGINACION",
            "PA_LISTA_CLIENTE",
            "PA_LISTA_CLIENTE_PAGINACION",
            "PA_LISTA_OFERTA",
            "PA_LISTA_OFERTA_PAGINACION",
            "PA_LISTA_PRODUCTO",
            "PA_LISTA_PRODUCTO_PAGINACION",
            "PA_LISTA_PROVEEDOR",
            "PA_LISTA_PROVEEDOR_PAGINACION",
            "PA_LISTA_SUCURSAL",
            "PA_LISTA_SUCURSAL_PAGINACION",
            "PA_LISTA_TRANSPORTISTA",
            "PA_LISTA_TRANSPORTISTA_PAGINACION",
            "PA_OBTENER_CATEGORIA",
            "PA_OBTENER_CLIENTE",
            "PA_OBTENER_NEGOCIO",
            "PA_OBTENER_OFERTA",
            "PA_OBTENER_PRODUCTO",
            "PA_OBTENER_PROVEEDOR",
            "PA_OBTENER_SUCURSAL",
            "PA_OBTENER_TRANSPORTISTA",
            "PA_REGISTRAR_CATEGORIA",
            "PA_REGISTRAR_CLIENTE",
            "PA_REGISTRAR_OFERTA",
            "PA_REGISTRAR_PRODUCTO",
            "PA_REGISTRAR_PROVEEDOR",
            "PA_REGISTRAR_SUCURSAL",
            "PA_REGISTRAR_TRANSPORTISTA"
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FECHA_REGISTRO -> FECHA_CREACION conservando los datos (EF lo habría generado como DROP + ADD)
            foreach (var tabla in TablasConFechaRegistro)
            {
                migrationBuilder.RenameColumn(name: "FECHA_REGISTRO", table: tabla, newName: "FECHA_CREACION");
            }

            // Antes de pasar FECHA_CREACION a NOT NULL se rellenan los registros antiguos sin fecha
            foreach (var tabla in TablasConFechaRegistro.Concat(new[] { "CATEGORIA", "OFERTA" }))
            {
                migrationBuilder.Sql($"UPDATE {tabla} SET FECHA_CREACION = SYSUTCDATETIME() WHERE FECHA_CREACION IS NULL;");
            }

            foreach (var tabla in TablasConFechaRegistro)
            {
                migrationBuilder.AlterColumn<DateTime>(
                    name: "FECHA_CREACION",
                    table: tabla,
                    type: "datetime2",
                    nullable: false,
                    defaultValueSql: "(sysutcdatetime())",
                    oldClrType: typeof(DateTime),
                    oldType: "datetime",
                    oldNullable: true,
                    oldDefaultValueSql: "(getdate())");
            }


            migrationBuilder.AddColumn<bool>(
                name: "ELIMINADO",
                table: "TRANSPORTISTA",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_MODIFICACION",
                table: "TRANSPORTISTA",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_CREACION",
                table: "TRANSPORTISTA",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_MODIFICACION",
                table: "TRANSPORTISTA",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ELIMINADO",
                table: "SUCURSAL",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "SUCURSAL",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_MODIFICACION",
                table: "SUCURSAL",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_CREACION",
                table: "SUCURSAL",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_MODIFICACION",
                table: "SUCURSAL",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ELIMINADO",
                table: "PROVEEDOR",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_MODIFICACION",
                table: "PROVEEDOR",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_CREACION",
                table: "PROVEEDOR",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_MODIFICACION",
                table: "PROVEEDOR",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ELIMINADO",
                table: "PRODUCTO",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "PRODUCTO",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_MODIFICACION",
                table: "PRODUCTO",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_CREACION",
                table: "PRODUCTO",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_MODIFICACION",
                table: "PRODUCTO",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "OFERTA",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<bool>(
                name: "ELIMINADO",
                table: "OFERTA",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_MODIFICACION",
                table: "OFERTA",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_CREACION",
                table: "OFERTA",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_MODIFICACION",
                table: "OFERTA",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "NEGOCIO",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_MODIFICACION",
                table: "NEGOCIO",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_CREACION",
                table: "NEGOCIO",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_MODIFICACION",
                table: "NEGOCIO",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ELIMINADO",
                table: "CLIENTE",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_MODIFICACION",
                table: "CLIENTE",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_CREACION",
                table: "CLIENTE",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_MODIFICACION",
                table: "CLIENTE",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "CATEGORIA",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<bool>(
                name: "ELIMINADO",
                table: "CATEGORIA",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_MODIFICACION",
                table: "CATEGORIA",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_CREACION",
                table: "CATEGORIA",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_MODIFICACION",
                table: "CATEGORIA",
                type: "int",
                nullable: true);

            // Stored procedures (CREATE OR ALTER) y datos iniciales: scripts embebidos en Migrations/Scripts
            foreach (var script in Scripts)
            {
                migrationBuilder.EjecutarScript(script);
            }

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var procedimiento in Procedimientos)
            {
                migrationBuilder.Sql($"DROP PROCEDURE IF EXISTS {procedimiento};");
            }

            migrationBuilder.DropColumn(
                name: "ELIMINADO",
                table: "TRANSPORTISTA");

            migrationBuilder.DropColumn(
                name: "FECHA_MODIFICACION",
                table: "TRANSPORTISTA");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_CREACION",
                table: "TRANSPORTISTA");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_MODIFICACION",
                table: "TRANSPORTISTA");

            migrationBuilder.DropColumn(
                name: "ELIMINADO",
                table: "SUCURSAL");

            migrationBuilder.DropColumn(
                name: "FECHA_CREACION",
                table: "SUCURSAL");

            migrationBuilder.DropColumn(
                name: "FECHA_MODIFICACION",
                table: "SUCURSAL");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_CREACION",
                table: "SUCURSAL");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_MODIFICACION",
                table: "SUCURSAL");

            migrationBuilder.DropColumn(
                name: "ELIMINADO",
                table: "PROVEEDOR");

            migrationBuilder.DropColumn(
                name: "FECHA_MODIFICACION",
                table: "PROVEEDOR");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_CREACION",
                table: "PROVEEDOR");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_MODIFICACION",
                table: "PROVEEDOR");

            migrationBuilder.DropColumn(
                name: "ELIMINADO",
                table: "PRODUCTO");

            migrationBuilder.DropColumn(
                name: "FECHA_CREACION",
                table: "PRODUCTO");

            migrationBuilder.DropColumn(
                name: "FECHA_MODIFICACION",
                table: "PRODUCTO");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_CREACION",
                table: "PRODUCTO");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_MODIFICACION",
                table: "PRODUCTO");

            migrationBuilder.DropColumn(
                name: "ELIMINADO",
                table: "OFERTA");

            migrationBuilder.DropColumn(
                name: "FECHA_MODIFICACION",
                table: "OFERTA");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_CREACION",
                table: "OFERTA");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_MODIFICACION",
                table: "OFERTA");

            migrationBuilder.DropColumn(
                name: "FECHA_CREACION",
                table: "NEGOCIO");

            migrationBuilder.DropColumn(
                name: "FECHA_MODIFICACION",
                table: "NEGOCIO");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_CREACION",
                table: "NEGOCIO");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_MODIFICACION",
                table: "NEGOCIO");

            migrationBuilder.DropColumn(
                name: "ELIMINADO",
                table: "CLIENTE");

            migrationBuilder.DropColumn(
                name: "FECHA_MODIFICACION",
                table: "CLIENTE");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_CREACION",
                table: "CLIENTE");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_MODIFICACION",
                table: "CLIENTE");

            migrationBuilder.DropColumn(
                name: "ELIMINADO",
                table: "CATEGORIA");

            migrationBuilder.DropColumn(
                name: "FECHA_MODIFICACION",
                table: "CATEGORIA");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_CREACION",
                table: "CATEGORIA");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_MODIFICACION",
                table: "CATEGORIA");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "OFERTA",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "CATEGORIA",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(sysutcdatetime())");

            foreach (var tabla in TablasConFechaRegistro)
            {
                migrationBuilder.AlterColumn<DateTime>(
                    name: "FECHA_CREACION",
                    table: tabla,
                    type: "datetime",
                    nullable: true,
                    defaultValueSql: "(getdate())",
                    oldClrType: typeof(DateTime),
                    oldType: "datetime2",
                    oldDefaultValueSql: "(sysutcdatetime())");

                migrationBuilder.RenameColumn(name: "FECHA_CREACION", table: tabla, newName: "FECHA_REGISTRO");
            }

        }
    }
}
