// SmartAgro.Data/SmartAgroDbContext.cs - VERSIÓN FINAL CORREGIDA
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
        public DbSet<MovimientoStock> MovimientosStock { get; set; }


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
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Apellidos).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Activo).HasDefaultValue(true);
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
                entity.Property(e => e.Activo).HasDefaultValue(true);
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
                entity.Property(e => e.CostoUnitario).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Activo).HasDefaultValue(true);

                entity.HasOne(e => e.Proveedor)
                      .WithMany(p => p.MateriasPrimas)
                      .HasForeignKey(e => e.ProveedorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.Nombre, e.ProveedorId })
                      .IsUnique()
                      .HasFilter("[Activo] = 1");
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
                entity.Property(e => e.PrecioBase).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PorcentajeGanancia).HasColumnType("decimal(5,2)");
                entity.Property(e => e.PrecioVenta).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ImagenPrincipal).HasMaxLength(500);
                entity.Property(e => e.ImagenesSecundarias).HasMaxLength(2000);
                entity.Property(e => e.VideoDemo).HasMaxLength(1000);
                entity.Property(e => e.Caracteristicas).HasMaxLength(2000);
                entity.Property(e => e.Beneficios).HasMaxLength(1000);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Nombre).IsUnique();
            });
        }

        private void ConfigurarProductoMateriaPrima(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductoMateriaPrima>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CantidadRequerida).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoUnitario).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostoTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notas).HasMaxLength(200);

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
                entity.Property(e => e.AreaCultivo).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TipoCultivo).HasMaxLength(50);
                entity.Property(e => e.TipoSuelo).HasMaxLength(50);
                entity.Property(e => e.RequierimientosEspeciales).HasMaxLength(1000);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PorcentajeImpuesto).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Impuestos).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Estado).HasMaxLength(20);
                entity.Property(e => e.Observaciones).HasMaxLength(1000);
                entity.Property(e => e.FechaCotizacion).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Usuario)
                      .WithMany()
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
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
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
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Impuestos).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
                entity.Property(e => e.EstadoVenta).HasMaxLength(20);
                entity.Property(e => e.MetodoPago).HasMaxLength(20);
                entity.Property(e => e.Observaciones).HasMaxLength(1000);
                entity.Property(e => e.NombreCliente).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EmailCliente).HasMaxLength(100);
                entity.Property(e => e.TelefonoCliente).HasMaxLength(20);
                entity.Property(e => e.DireccionEntrega).HasMaxLength(500);
                entity.Property(e => e.FechaVenta).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Usuario)
                      .WithMany()
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
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");

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
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
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
                entity.Property(e => e.Cantidad).HasColumnType("decimal(18,4)");
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notas).HasMaxLength(200);

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

        private void ConfigurarMovimientoStock(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MovimientoStock>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Tipo).HasMaxLength(20);
                entity.Property(e => e.CostoUnitario).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Cantidad).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Fecha).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.MateriaPrima)
                      .WithMany()
                      .HasForeignKey(e => e.MateriaPrimaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigurarComentario(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comentario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Calificacion).HasDefaultValue(5);
                entity.Property(e => e.Contenido).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.RespuestaAdmin).HasMaxLength(2000);
                entity.Property(e => e.Aprobado).HasDefaultValue(false);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaComentario).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Producto)
                      .WithMany(p => p.Comentarios)
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasCheckConstraint("CK_Comentario_Calificacion", "[Calificacion] >= 1 AND [Calificacion] <= 5");
            });
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Datos semilla - igual que antes
            // (mantén el mismo código de semilla que tenías)
        }
    }
}