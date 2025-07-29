using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartAgro.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVentaEntityWithClientData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_Ventas_VentaId",
                table: "Comentarios");

            migrationBuilder.DropIndex(
                name: "IX_MateriasPrimas_Nombre_ProveedorId",
                table: "MateriasPrimas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Comentario_Calificacion",
                table: "Comentarios");

            migrationBuilder.DeleteData(
                table: "ProductoMateriasPrimas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProductoMateriasPrimas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProductoMateriasPrimas",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ProductoMateriasPrimas",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ProductoMateriasPrimas",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ProductoMateriasPrimas",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ProductoMateriasPrimas",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ProductoMateriasPrimas",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ProductoMateriasPrimas",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ProductoMateriasPrimas",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ProductoMateriasPrimas",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ProductoMateriasPrimas",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "MateriasPrimas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MateriasPrimas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MateriasPrimas",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MateriasPrimas",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MateriasPrimas",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MateriasPrimas",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MateriasPrimas",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "MateriasPrimas",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "MateriasPrimas",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "MateriasPrimas",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "MateriasPrimas",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "MateriasPrimas",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AlterColumn<string>(
                name: "MetodoPago",
                table: "Ventas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "DireccionEntrega",
                table: "Ventas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailCliente",
                table: "Ventas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreCliente",
                table: "Ventas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TelefonoCliente",
                table: "Ventas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Proveedores",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "MateriasPrimas",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                table: "DetallesCompraProveedor",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "DetallesCompraProveedor",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoSuelo",
                table: "Cotizaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "TipoCultivo",
                table: "Cotizaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "RespuestaAdmin",
                table: "Comentarios",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Contenido",
                table: "Comentarios",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

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

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_MateriasPrimas_Nombre_ProveedorId",
                table: "MateriasPrimas",
                columns: new[] { "Nombre", "ProveedorId" },
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Comentario_Calificacion",
                table: "Comentarios",
                sql: "[Calificacion] >= 1 AND [Calificacion] <= 5");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_Ventas_VentaId",
                table: "Comentarios",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_Ventas_VentaId",
                table: "Comentarios");

            migrationBuilder.DropIndex(
                name: "IX_MateriasPrimas_Nombre_ProveedorId",
                table: "MateriasPrimas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Comentario_Calificacion",
                table: "Comentarios");

            migrationBuilder.DropColumn(
                name: "DireccionEntrega",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "EmailCliente",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "NombreCliente",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "TelefonoCliente",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "DetallesCompraProveedor");

            migrationBuilder.AlterColumn<string>(
                name: "MetodoPago",
                table: "Ventas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Proveedores",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Productos",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "MateriasPrimas",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "Cantidad",
                table: "DetallesCompraProveedor",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "TipoSuelo",
                table: "Cotizaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoCultivo",
                table: "Cotizaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RespuestaAdmin",
                table: "Comentarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Contenido",
                table: "Comentarios",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

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

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Activo", "Beneficios", "Caracteristicas", "Descripcion", "DescripcionDetallada", "FechaCreacion", "ImagenPrincipal", "ImagenesSecundarias", "Nombre", "PorcentajeGanancia", "PrecioBase", "PrecioVenta", "VideoDemo" },
                values: new object[] { 1, true, "[\"Ahorro de agua hasta 40%\", \"Mejora la salud de cultivos\", \"Reduce tiempo de mantenimiento\", \"Control remoto total\", \"Datos históricos para análisis\", \"Escalable a múltiples zonas\"]", "[\"Monitoreo 24/7 automático\", \"Sensores de humedad de precisión\", \"Control de calidad del aire\", \"App móvil Android nativa\", \"Conectividad WiFi\", \"Dashboard web\", \"Notificaciones en tiempo real\", \"Configuración personalizable\", \"Resistente a intemperie IP65\", \"Fácil instalación\"]", "Sistema IoT completo para automatización de riego con monitoreo de humedad del suelo y calidad del aire", "El Sistema de Riego Automático Inteligente de SmartAgro IoT Solutions es una solución integral que combina tecnología IoT de vanguardia con sensores de precisión para optimizar el riego de cultivos y espacios verdes. \r\n\r\nEl sistema incluye:\r\n- Monitoreo continuo de humedad del suelo mediante sensores calibrados\r\n- Seguimiento de calidad del aire en tiempo real\r\n- Control automático de riego basado en parámetros configurables\r\n- Aplicación móvil Android para control y monitoreo remoto\r\n- Conectividad WiFi para acceso desde cualquier lugar\r\n- Dashboard con análisis de datos históricos\r\n- Alertas y notificaciones push en tiempo real\r\n\r\nIdeal para agricultores, jardineros profesionales y entusiastas de la jardinería que buscan optimizar el uso del agua y mejorar la salud de sus cultivos mediante tecnología inteligente.", new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "/images/sistema-riego-principal.jpg", "[\"/images/sistema-riego-1.jpg\", \"/images/sistema-riego-2.jpg\", \"/images/sistema-riego-3.jpg\", \"/images/app-mobile.jpg\"]", "Sistema de Riego Automático Inteligente", 35.00m, 2500.00m, 3375.00m, "https://www.youtube.com/embed/demo-video-id" });

            migrationBuilder.InsertData(
                table: "Proveedores",
                columns: new[] { "Id", "Activo", "ContactoPrincipal", "Direccion", "Email", "FechaRegistro", "Nombre", "RFC", "RazonSocial", "Telefono" },
                values: new object[,]
                {
                    { 1, true, "Juan Pérez", "Av. Industrial 123, León, Gto", "ventas@techcomponents.com", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "TechComponents SA", "TCO850101ABC", "TechComponents SA de CV", "477-123-4567" },
                    { 2, true, "María González", "Calle Electrónica 456, León, Gto", "contacto@electrosupply.mx", new DateTime(2024, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "ElectroSupply MX", "ESM920201DEF", "ElectroSupply México SA de CV", "477-234-5678" },
                    { 3, true, "Carlos Martínez", "Blvd. Agricultura 789, León, Gto", "info@agrotech-dist.com", new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "AgroTech Distribuidores", "ATD780301GHI", "AgroTech Distribuidores SA de CV", "477-345-6789" }
                });

            migrationBuilder.InsertData(
                table: "MateriasPrimas",
                columns: new[] { "Id", "Activo", "CostoUnitario", "Descripcion", "Nombre", "ProveedorId", "Stock", "StockMinimo", "UnidadMedida" },
                values: new object[,]
                {
                    { 1, true, 450.00m, "Microcontrolador Arduino Uno Rev3", "Arduino Uno R3", 1, 50, 10, "Pieza" },
                    { 2, true, 85.00m, "Sensor de humedad de suelo resistivo", "Sensor Humedad Suelo", 1, 100, 20, "Pieza" },
                    { 3, true, 120.00m, "Sensor de calidad de aire MQ-135", "Sensor Calidad Aire MQ-135", 2, 75, 15, "Pieza" },
                    { 4, true, 180.00m, "Módulo WiFi ESP8266 NodeMCU", "Módulo WiFi ESP8266", 2, 60, 12, "Pieza" },
                    { 5, true, 45.00m, "Relé electromecánico 5V 10A", "Relé 5V 10A", 2, 80, 16, "Pieza" },
                    { 6, true, 320.00m, "Electroválvula solenoide 12V 1/2 pulgada", "Electroválvula 12V", 3, 40, 8, "Pieza" },
                    { 7, true, 280.00m, "Bomba de agua sumergible 12V 5L/min", "Bomba de Agua 12V", 3, 30, 6, "Pieza" },
                    { 8, true, 150.00m, "Fuente de alimentación 12V 2A", "Fuente 12V 2A", 2, 45, 9, "Pieza" },
                    { 9, true, 220.00m, "Caja hermética para exteriores IP65", "Caja Protección IP65", 1, 35, 7, "Pieza" },
                    { 10, true, 12.50m, "Cable multicore 6 hilos calibre 20 AWG", "Cable Multicore 10m", 2, 200, 40, "Metro" },
                    { 11, true, 65.00m, "Set conectores impermeables IP67", "Conectores Impermeables", 1, 70, 14, "Set" },
                    { 12, true, 8.50m, "Manguera para riego 1/2 pulgada", "Manguera Riego 1/2\"", 3, 300, 60, "Metro" }
                });

            migrationBuilder.InsertData(
                table: "ProductoMateriasPrimas",
                columns: new[] { "Id", "CantidadRequerida", "CostoTotal", "CostoUnitario", "MateriaPrimaId", "Notas", "ProductoId" },
                values: new object[,]
                {
                    { 1, 1m, 450.00m, 450.00m, 1, "Controlador principal del sistema", 1 },
                    { 2, 2m, 170.00m, 85.00m, 2, "Sensores para diferentes zonas", 1 },
                    { 3, 1m, 120.00m, 120.00m, 3, "Monitoreo calidad aire", 1 },
                    { 4, 1m, 180.00m, 180.00m, 4, "Conectividad WiFi", 1 },
                    { 5, 2m, 90.00m, 45.00m, 5, "Control electroválvulas", 1 },
                    { 6, 2m, 640.00m, 320.00m, 6, "Válvulas para riego", 1 },
                    { 7, 1m, 280.00m, 280.00m, 7, "Bomba principal", 1 },
                    { 8, 1m, 150.00m, 150.00m, 8, "Alimentación sistema", 1 },
                    { 9, 1m, 220.00m, 220.00m, 9, "Protección intemperie", 1 },
                    { 10, 15m, 187.50m, 12.50m, 10, "Cableado sistema", 1 },
                    { 11, 1m, 65.00m, 65.00m, 11, "Conexiones seguras", 1 },
                    { 12, 20m, 170.00m, 8.50m, 12, "Mangueras distribución", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MateriasPrimas_Nombre_ProveedorId",
                table: "MateriasPrimas",
                columns: new[] { "Nombre", "ProveedorId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Comentario_Calificacion",
                table: "Comentarios",
                sql: "Calificacion >= 1 AND Calificacion <= 5");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_Ventas_VentaId",
                table: "Comentarios",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
