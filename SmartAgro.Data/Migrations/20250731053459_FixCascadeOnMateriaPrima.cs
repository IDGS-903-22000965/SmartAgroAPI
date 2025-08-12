using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAgro.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeOnMateriaPrima : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_AspNetUsers_UsuarioId",
                table: "Comentarios");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasProveedores_Proveedores_ProveedorId",
                table: "ComprasProveedores");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesCompraProveedor_MateriasPrimas_MateriaPrimaId",
                table: "DetallesCompraProveedor");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesCotizacion_Productos_ProductoId",
                table: "DetallesCotizacion");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Productos_ProductoId",
                table: "DetallesVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductoMateriasPrimas_MateriasPrimas_MateriaPrimaId",
                table: "ProductoMateriasPrimas");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_Email",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_RFC",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Nombre",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_ProductoMateriasPrimas_ProductoId_MateriaPrimaId",
                table: "ProductoMateriasPrimas");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosStock_MateriaPrimaId",
                table: "MovimientosStock");

            migrationBuilder.DropIndex(
                name: "IX_MateriasPrimas_Nombre_ProveedorId",
                table: "MateriasPrimas");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_NumeroCotizacion",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_ComprasProveedores_NumeroCompra",
                table: "ComprasProveedores");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Comentario_Calificacion",
                table: "Comentarios");

            migrationBuilder.RenameIndex(
                name: "IX_Ventas_NumeroVenta",
                table: "Ventas",
                newName: "IX_Venta_NumeroVenta");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaVenta",
                table: "Ventas",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaRegistro",
                table: "Proveedores",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Proveedores",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCreacion",
                table: "Productos",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Productos",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                table: "MovimientosStock",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "MateriasPrimas",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCotizacion",
                table: "Cotizaciones",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCompra",
                table: "ComprasProveedores",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaComentario",
                table: "Comentarios",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "Calificacion",
                table: "Comentarios",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AlterColumn<bool>(
                name: "Aprobado",
                table: "Comentarios",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Comentarios",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaRegistro",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Venta_FechaVenta",
                table: "Ventas",
                column: "FechaVenta");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoMateriasPrimas_ProductoId",
                table: "ProductoMateriasPrimas",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoStock_MateriaPrima_Fecha",
                table: "MovimientosStock",
                columns: new[] { "MateriaPrimaId", "Fecha" });

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_AspNetUsers_UsuarioId",
                table: "Comentarios",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasProveedores_Proveedores_ProveedorId",
                table: "ComprasProveedores",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesCompraProveedor_MateriasPrimas_MateriaPrimaId",
                table: "DetallesCompraProveedor",
                column: "MateriaPrimaId",
                principalTable: "MateriasPrimas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesCotizacion_Productos_ProductoId",
                table: "DetallesCotizacion",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Productos_ProductoId",
                table: "DetallesVenta",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoMateriasPrimas_MateriasPrimas_MateriaPrimaId",
                table: "ProductoMateriasPrimas",
                column: "MateriaPrimaId",
                principalTable: "MateriasPrimas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_AspNetUsers_UsuarioId",
                table: "Comentarios");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasProveedores_Proveedores_ProveedorId",
                table: "ComprasProveedores");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesCompraProveedor_MateriasPrimas_MateriaPrimaId",
                table: "DetallesCompraProveedor");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesCotizacion_Productos_ProductoId",
                table: "DetallesCotizacion");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Productos_ProductoId",
                table: "DetallesVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductoMateriasPrimas_MateriasPrimas_MateriaPrimaId",
                table: "ProductoMateriasPrimas");

            migrationBuilder.DropIndex(
                name: "IX_Venta_FechaVenta",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_ProductoMateriasPrimas_ProductoId",
                table: "ProductoMateriasPrimas");

            migrationBuilder.DropIndex(
                name: "IX_MovimientoStock_MateriaPrima_Fecha",
                table: "MovimientosStock");

            migrationBuilder.RenameIndex(
                name: "IX_Venta_NumeroVenta",
                table: "Ventas",
                newName: "IX_Ventas_NumeroVenta");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaVenta",
                table: "Ventas",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaRegistro",
                table: "Proveedores",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Proveedores",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCreacion",
                table: "Productos",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                table: "MovimientosStock",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "MateriasPrimas",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCotizacion",
                table: "Cotizaciones",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCompra",
                table: "ComprasProveedores",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaComentario",
                table: "Comentarios",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "Calificacion",
                table: "Comentarios",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "Aprobado",
                table: "Comentarios",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Comentarios",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaRegistro",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_Email",
                table: "Proveedores",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_RFC",
                table: "Proveedores",
                column: "RFC",
                unique: true,
                filter: "[RFC] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Nombre",
                table: "Productos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductoMateriasPrimas_ProductoId_MateriaPrimaId",
                table: "ProductoMateriasPrimas",
                columns: new[] { "ProductoId", "MateriaPrimaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_MateriaPrimaId",
                table: "MovimientosStock",
                column: "MateriaPrimaId");

            migrationBuilder.CreateIndex(
                name: "IX_MateriasPrimas_Nombre_ProveedorId",
                table: "MateriasPrimas",
                columns: new[] { "Nombre", "ProveedorId" },
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_NumeroCotizacion",
                table: "Cotizaciones",
                column: "NumeroCotizacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprasProveedores_NumeroCompra",
                table: "ComprasProveedores",
                column: "NumeroCompra",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Comentario_Calificacion",
                table: "Comentarios",
                sql: "[Calificacion] >= 1 AND [Calificacion] <= 5");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_AspNetUsers_UsuarioId",
                table: "Comentarios",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasProveedores_Proveedores_ProveedorId",
                table: "ComprasProveedores",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesCompraProveedor_MateriasPrimas_MateriaPrimaId",
                table: "DetallesCompraProveedor",
                column: "MateriaPrimaId",
                principalTable: "MateriasPrimas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesCotizacion_Productos_ProductoId",
                table: "DetallesCotizacion",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Productos_ProductoId",
                table: "DetallesVenta",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoMateriasPrimas_MateriasPrimas_MateriaPrimaId",
                table: "ProductoMateriasPrimas",
                column: "MateriaPrimaId",
                principalTable: "MateriasPrimas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
