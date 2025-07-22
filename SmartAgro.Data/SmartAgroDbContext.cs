using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Models.Entities;

namespace SmartAgro.Data
{
    public class SmartAgroDbContext : IdentityDbContext<Usuario>
    {
        public SmartAgroDbContext(DbContextOptions<SmartAgroDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<MateriaPrima> MateriasPrimas { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ProductoMateriaPrima> ProductoMateriasPrimas { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<DetalleCotizacion> DetallesCotizacion { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<CompraProveedor> ComprasProveedores { get; set; }
        public DbSet<DetalleCompraProveedor> DetallesCompraProveedor { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones de entidades
            ConfigurarUsuario(modelBuilder);
            ConfigurarProveedor(modelBuilder);
            ConfigurarMateriaPrima(modelBuilder);
            ConfigurarProducto(modelBuilder);
            ConfigurarProductoMateriaPrima(modelBuilder);
            ConfigurarCotizacion(modelBuilder);
            ConfigurarDetalleCotizacion(modelBuilder);
            ConfigurarVenta(modelBuilder);
            ConfigurarDetalleVenta(modelBuilder);
            ConfigurarCompraProveedor(modelBuilder);
            ConfigurarDetalleCompraProveedor(modelBuilder);
            ConfigurarComentario(modelBuilder);

            // Datos semilla
            SeedData(modelBuilder);
        }

        private void ConfigurarUsuario(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellidos).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");
            });
        }

        private void ConfigurarProveedor(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RazonSocial).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RFC).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.ContactoPrincipal).HasMaxLength(100);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.RFC).IsUnique().HasFilter("[RFC] IS NOT NULL");
                entity.HasIndex(e => e.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            });
        }

        private void ConfigurarMateriaPrima(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MateriaPrima>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.UnidadMedida).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CostoUnitario).HasPrecision(18, 2);

                entity.HasOne(e => e.Proveedor)
                      .WithMany(p => p.MateriasPrimas)
                      .HasForeignKey(e => e.ProveedorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.Nombre, e.ProveedorId }).IsUnique();
            });
        }

        private void ConfigurarProducto(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(1000);
                entity.Property(e => e.DescripcionDetallada).HasMaxLength(2000);
                entity.Property(e => e.PrecioBase).HasPrecision(18, 2);
                entity.Property(e => e.PorcentajeGanancia).HasPrecision(5, 2);
                entity.Property(e => e.PrecioVenta).HasPrecision(18, 2);
                entity.Property(e => e.ImagenPrincipal).HasMaxLength(500);
                entity.Property(e => e.ImagenesSecundarias).HasMaxLength(2000);
                entity.Property(e => e.VideoDemo).HasMaxLength(1000);
                entity.Property(e => e.Caracteristicas).HasMaxLength(2000);
                entity.Property(e => e.Beneficios).HasMaxLength(1000);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Nombre).IsUnique();
            });
        }

        private void ConfigurarProductoMateriaPrima(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductoMateriaPrima>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CantidadRequerida).HasPrecision(18, 4);
                entity.Property(e => e.CostoUnitario).HasPrecision(18, 2);
                entity.Property(e => e.CostoTotal).HasPrecision(18, 2);

                entity.HasOne(e => e.Producto)
                      .WithMany(p => p.ProductoMateriasPrimas)
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.MateriaPrima)
                      .WithMany(m => m.ProductoMateriasPrimas)
                      .HasForeignKey(e => e.MateriaPrimaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.ProductoId, e.MateriaPrimaId }).IsUnique();
            });
        }

        private void ConfigurarCotizacion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cotizacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroCotizacion).IsRequired().HasMaxLength(20);
                entity.Property(e => e.NombreCliente).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EmailCliente).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TelefonoCliente).HasMaxLength(20);
                entity.Property(e => e.DireccionInstalacion).HasMaxLength(200);
                entity.Property(e => e.AreaCultivo).HasPrecision(10, 2);
                entity.Property(e => e.TipoCultivo).HasMaxLength(50);
                entity.Property(e => e.TipoSuelo).HasMaxLength(50);
                entity.Property(e => e.RequierimientosEspeciales).HasMaxLength(1000);
                entity.Property(e => e.Subtotal).HasPrecision(18, 2);
                entity.Property(e => e.PorcentajeImpuesto).HasPrecision(5, 2);
                entity.Property(e => e.Impuestos).HasPrecision(18, 2);
                entity.Property(e => e.Total).HasPrecision(18, 2);
                entity.Property(e => e.Estado).HasMaxLength(20);
                entity.Property(e => e.Observaciones).HasMaxLength(1000);
                entity.Property(e => e.FechaCotizacion).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.Cotizaciones)
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.NumeroCotizacion).IsUnique();
            });
        }

        private void ConfigurarDetalleCotizacion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DetalleCotizacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
                entity.Property(e => e.Subtotal).HasPrecision(18, 2);
                entity.Property(e => e.Descripcion).HasMaxLength(500);

                entity.HasOne(e => e.Cotizacion)
                      .WithMany(c => c.Detalles)
                      .HasForeignKey(e => e.CotizacionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Producto)
                      .WithMany(p => p.DetallesCotizacion)
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarVenta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroVenta).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Subtotal).HasPrecision(18, 2);
                entity.Property(e => e.Impuestos).HasPrecision(18, 2);
                entity.Property(e => e.Total).HasPrecision(18, 2);
                entity.Property(e => e.EstadoVenta).HasMaxLength(20);
                entity.Property(e => e.MetodoPago).HasMaxLength(20);
                entity.Property(e => e.Observaciones).HasMaxLength(1000);
                entity.Property(e => e.FechaVenta).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.Compras)
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Cotizacion)
                      .WithMany()
                      .HasForeignKey(e => e.CotizacionId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.NumeroVenta).IsUnique();
            });
        }

        private void ConfigurarDetalleVenta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
                entity.Property(e => e.Subtotal).HasPrecision(18, 2);

                entity.HasOne(e => e.Venta)
                      .WithMany(v => v.Detalles)
                      .HasForeignKey(e => e.VentaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Producto)
                      .WithMany(p => p.DetallesVenta)
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarCompraProveedor(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompraProveedor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroCompra).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Total).HasPrecision(18, 2);
                entity.Property(e => e.Estado).HasMaxLength(20);
                entity.Property(e => e.Observaciones).HasMaxLength(1000);
                entity.Property(e => e.FechaCompra).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Proveedor)
                      .WithMany(p => p.Compras)
                      .HasForeignKey(e => e.ProveedorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.NumeroCompra).IsUnique();
            });
        }

        private void ConfigurarDetalleCompraProveedor(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DetalleCompraProveedor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
                entity.Property(e => e.Subtotal).HasPrecision(18, 2);

                entity.HasOne(e => e.CompraProveedor)
                      .WithMany(c => c.Detalles)
                      .HasForeignKey(e => e.CompraProveedorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.MateriaPrima)
                      .WithMany(m => m.DetallesCompra)
                      .HasForeignKey(e => e.MateriaPrimaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarComentario(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comentario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Contenido).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.RespuestaAdmin).HasMaxLength(500);
                entity.Property(e => e.FechaComentario).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.Comentarios)
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Producto)
                      .WithMany(p => p.Comentarios)
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Venta)
                      .WithMany()
                      .HasForeignKey(e => e.VentaId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasCheckConstraint("CK_Comentario_Calificacion", "Calificacion >= 1 AND Calificacion <= 5");
            });
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Proveedores
            modelBuilder.Entity<Proveedor>().HasData(
                new Proveedor
                {
                    Id = 1,
                    Nombre = "TechComponents SA",
                    RazonSocial = "TechComponents SA de CV",
                    RFC = "TCO850101ABC",
                    Direccion = "Av. Industrial 123, León, Gto",
                    Telefono = "477-123-4567",
                    Email = "ventas@techcomponents.com",
                    ContactoPrincipal = "Juan Pérez",
                    Activo = true,
                    FechaRegistro = new DateTime(2024, 1, 15)
                },
                new Proveedor
                {
                    Id = 2,
                    Nombre = "ElectroSupply MX",
                    RazonSocial = "ElectroSupply México SA de CV",
                    RFC = "ESM920201DEF",
                    Direccion = "Calle Electrónica 456, León, Gto",
                    Telefono = "477-234-5678",
                    Email = "contacto@electrosupply.mx",
                    ContactoPrincipal = "María González",
                    Activo = true,
                    FechaRegistro = new DateTime(2024, 2, 10)
                },
                new Proveedor
                {
                    Id = 3,
                    Nombre = "AgroTech Distribuidores",
                    RazonSocial = "AgroTech Distribuidores SA de CV",
                    RFC = "ATD780301GHI",
                    Direccion = "Blvd. Agricultura 789, León, Gto",
                    Telefono = "477-345-6789",
                    Email = "info@agrotech-dist.com",
                    ContactoPrincipal = "Carlos Martínez",
                    Activo = true,
                    FechaRegistro = new DateTime(2024, 1, 20)
                }
            );

            // Seed Materias Primas
            modelBuilder.Entity<MateriaPrima>().HasData(
                // Componentes electrónicos
                new MateriaPrima { Id = 1, Nombre = "Arduino Uno R3", Descripcion = "Microcontrolador Arduino Uno Rev3", UnidadMedida = "Pieza", CostoUnitario = 450.00m, Stock = 50, StockMinimo = 10, ProveedorId = 1, Activo = true },
                new MateriaPrima { Id = 2, Nombre = "Sensor Humedad Suelo", Descripcion = "Sensor de humedad de suelo resistivo", UnidadMedida = "Pieza", CostoUnitario = 85.00m, Stock = 100, StockMinimo = 20, ProveedorId = 1, Activo = true },
                new MateriaPrima { Id = 3, Nombre = "Sensor Calidad Aire MQ-135", Descripcion = "Sensor de calidad de aire MQ-135", UnidadMedida = "Pieza", CostoUnitario = 120.00m, Stock = 75, StockMinimo = 15, ProveedorId = 2, Activo = true },
                new MateriaPrima { Id = 4, Nombre = "Módulo WiFi ESP8266", Descripcion = "Módulo WiFi ESP8266 NodeMCU", UnidadMedida = "Pieza", CostoUnitario = 180.00m, Stock = 60, StockMinimo = 12, ProveedorId = 2, Activo = true },
                new MateriaPrima { Id = 5, Nombre = "Relé 5V 10A", Descripcion = "Relé electromecánico 5V 10A", UnidadMedida = "Pieza", CostoUnitario = 45.00m, Stock = 80, StockMinimo = 16, ProveedorId = 2, Activo = true },
                new MateriaPrima { Id = 6, Nombre = "Electroválvula 12V", Descripcion = "Electroválvula solenoide 12V 1/2 pulgada", UnidadMedida = "Pieza", CostoUnitario = 320.00m, Stock = 40, StockMinimo = 8, ProveedorId = 3, Activo = true },
                new MateriaPrima { Id = 7, Nombre = "Bomba de Agua 12V", Descripcion = "Bomba de agua sumergible 12V 5L/min", UnidadMedida = "Pieza", CostoUnitario = 280.00m, Stock = 30, StockMinimo = 6, ProveedorId = 3, Activo = true },
                new MateriaPrima { Id = 8, Nombre = "Fuente 12V 2A", Descripcion = "Fuente de alimentación 12V 2A", UnidadMedida = "Pieza", CostoUnitario = 150.00m, Stock = 45, StockMinimo = 9, ProveedorId = 2, Activo = true },
                new MateriaPrima { Id = 9, Nombre = "Caja Protección IP65", Descripcion = "Caja hermética para exteriores IP65", UnidadMedida = "Pieza", CostoUnitario = 220.00m, Stock = 35, StockMinimo = 7, ProveedorId = 1, Activo = true },
                new MateriaPrima { Id = 10, Nombre = "Cable Multicore 10m", Descripcion = "Cable multicore 6 hilos calibre 20 AWG", UnidadMedida = "Metro", CostoUnitario = 12.50m, Stock = 200, StockMinimo = 40, ProveedorId = 2, Activo = true },
                new MateriaPrima { Id = 11, Nombre = "Conectores Impermeables", Descripcion = "Set conectores impermeables IP67", UnidadMedida = "Set", CostoUnitario = 65.00m, Stock = 70, StockMinimo = 14, ProveedorId = 1, Activo = true },
                new MateriaPrima { Id = 12, Nombre = "Manguera Riego 1/2\"", Descripcion = "Manguera para riego 1/2 pulgada", UnidadMedida = "Metro", CostoUnitario = 8.50m, Stock = 300, StockMinimo = 60, ProveedorId = 3, Activo = true }
            );

            // Seed Producto Principal
            modelBuilder.Entity<Producto>().HasData(
                new Producto
                {
                    Id = 1,
                    Nombre = "Sistema de Riego Automático Inteligente",
                    Descripcion = "Sistema IoT completo para automatización de riego con monitoreo de humedad del suelo y calidad del aire",
                    DescripcionDetallada = @"El Sistema de Riego Automático Inteligente de SmartAgro IoT Solutions es una solución integral que combina tecnología IoT de vanguardia con sensores de precisión para optimizar el riego de cultivos y espacios verdes. 

El sistema incluye:
- Monitoreo continuo de humedad del suelo mediante sensores calibrados
- Seguimiento de calidad del aire en tiempo real
- Control automático de riego basado en parámetros configurables
- Aplicación móvil Android para control y monitoreo remoto
- Conectividad WiFi para acceso desde cualquier lugar
- Dashboard con análisis de datos históricos
- Alertas y notificaciones push en tiempo real

Ideal para agricultores, jardineros profesionales y entusiastas de la jardinería que buscan optimizar el uso del agua y mejorar la salud de sus cultivos mediante tecnología inteligente.",
                    PrecioBase = 2500.00m,
                    PorcentajeGanancia = 35.00m,
                    PrecioVenta = 3375.00m,
                    ImagenPrincipal = "/images/sistema-riego-principal.jpg",
                    ImagenesSecundarias = @"[""/images/sistema-riego-1.jpg"", ""/images/sistema-riego-2.jpg"", ""/images/sistema-riego-3.jpg"", ""/images/app-mobile.jpg""]",
                    VideoDemo = "https://www.youtube.com/embed/demo-video-id",
                    Caracteristicas = @"[""Monitoreo 24/7 automático"", ""Sensores de humedad de precisión"", ""Control de calidad del aire"", ""App móvil Android nativa"", ""Conectividad WiFi"", ""Dashboard web"", ""Notificaciones en tiempo real"", ""Configuración personalizable"", ""Resistente a intemperie IP65"", ""Fácil instalación""]",
                    Beneficios = @"[""Ahorro de agua hasta 40%"", ""Mejora la salud de cultivos"", ""Reduce tiempo de mantenimiento"", ""Control remoto total"", ""Datos históricos para análisis"", ""Escalable a múltiples zonas""]",
                    Activo = true,
                    FechaCreacion = new DateTime(2024, 3, 1)
                }
            );

            // Seed BOM (Bill of Materials) - Explosión de Materiales
            modelBuilder.Entity<ProductoMateriaPrima>().HasData(
                new ProductoMateriaPrima { Id = 1, ProductoId = 1, MateriaPrimaId = 1, CantidadRequerida = 1, CostoUnitario = 450.00m, CostoTotal = 450.00m, Notas = "Controlador principal del sistema" },
                new ProductoMateriaPrima { Id = 2, ProductoId = 1, MateriaPrimaId = 2, CantidadRequerida = 2, CostoUnitario = 85.00m, CostoTotal = 170.00m, Notas = "Sensores para diferentes zonas" },
                new ProductoMateriaPrima { Id = 3, ProductoId = 1, MateriaPrimaId = 3, CantidadRequerida = 1, CostoUnitario = 120.00m, CostoTotal = 120.00m, Notas = "Monitoreo calidad aire" },
                new ProductoMateriaPrima { Id = 4, ProductoId = 1, MateriaPrimaId = 4, CantidadRequerida = 1, CostoUnitario = 180.00m, CostoTotal = 180.00m, Notas = "Conectividad WiFi" },
                new ProductoMateriaPrima { Id = 5, ProductoId = 1, MateriaPrimaId = 5, CantidadRequerida = 2, CostoUnitario = 45.00m, CostoTotal = 90.00m, Notas = "Control electroválvulas" },
                new ProductoMateriaPrima { Id = 6, ProductoId = 1, MateriaPrimaId = 6, CantidadRequerida = 2, CostoUnitario = 320.00m, CostoTotal = 640.00m, Notas = "Válvulas para riego" },
                new ProductoMateriaPrima { Id = 7, ProductoId = 1, MateriaPrimaId = 7, CantidadRequerida = 1, CostoUnitario = 280.00m, CostoTotal = 280.00m, Notas = "Bomba principal" },
                new ProductoMateriaPrima { Id = 8, ProductoId = 1, MateriaPrimaId = 8, CantidadRequerida = 1, CostoUnitario = 150.00m, CostoTotal = 150.00m, Notas = "Alimentación sistema" },
                new ProductoMateriaPrima { Id = 9, ProductoId = 1, MateriaPrimaId = 9, CantidadRequerida = 1, CostoUnitario = 220.00m, CostoTotal = 220.00m, Notas = "Protección intemperie" },
                new ProductoMateriaPrima { Id = 10, ProductoId = 1, MateriaPrimaId = 10, CantidadRequerida = 15, CostoUnitario = 12.50m, CostoTotal = 187.50m, Notas = "Cableado sistema" },
                new ProductoMateriaPrima { Id = 11, ProductoId = 1, MateriaPrimaId = 11, CantidadRequerida = 1, CostoUnitario = 65.00m, CostoTotal = 65.00m, Notas = "Conexiones seguras" },
                new ProductoMateriaPrima { Id = 12, ProductoId = 1, MateriaPrimaId = 12, CantidadRequerida = 20, CostoUnitario = 8.50m, CostoTotal = 170.00m, Notas = "Mangueras distribución" }
            );
        }
    }
}