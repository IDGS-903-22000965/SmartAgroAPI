using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Models.Entities;

namespace SmartAgro.Data
{
    public class SmartAgroDbContext : IdentityDbContext<Usuario>
    {
        public SmartAgroDbContext(DbContextOptions<SmartAgroDbContext> options) : base(options) { }

        // ✅ DBSETS
        public DbSet<Producto> Productos { get; set; }
        public DbSet<MateriaPrima> MateriasPrimas { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<ProductoMateriaPrima> ProductoMateriasPrimas { get; set; }
        public DbSet<CompraProveedor> ComprasProveedores { get; set; }
        public DbSet<DetalleCompraProveedor> DetallesCompraProveedor { get; set; }
        public DbSet<MovimientoStock> MovimientosStock { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<DetalleCotizacion> DetallesCotizacion { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<ProductoDocumento> ProductoDocumentos { get; set; } 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductoDocumento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Url).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.EsVisibleParaCliente).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.Producto)
                      .WithMany()
                      .HasForeignKey(d => d.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Índice para mejorar rendimiento en consultas
                entity.HasIndex(e => new { e.ProductoId, e.EsVisibleParaCliente })
                      .HasDatabaseName("IX_ProductoDocumento_ProductoId_Visible");
            });
            // ✅ CONFIGURAR DECIMALES CORRECTAMENTE
            modelBuilder.Entity<MovimientoStock>()
                .Property(e => e.Cantidad)
                .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<MovimientoStock>()
                .Property(e => e.CostoUnitario)
                .HasColumnType("decimal(18,2)");

            // ✅ CONFIGURAR RELACIONES
            modelBuilder.Entity<Usuario>()
                .HasMany<Cotizacion>()
                .WithOne(c => c.Usuario)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Usuario>()
                .HasMany<Venta>()
                .WithOne(v => v.Usuario)
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cotizacion>()
                .HasMany(c => c.Detalles)
                .WithOne(d => d.Cotizacion)
                .HasForeignKey(d => d.CotizacionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Producto>()
                .HasMany(p => p.ProductoMateriasPrimas)
                .WithOne(pm => pm.Producto)
                .HasForeignKey(pm => pm.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MateriaPrima>()
                .HasOne(mp => mp.Proveedor)
    .WithMany(p => p.MateriasPrimas)
    .HasForeignKey(mp => mp.ProveedorId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Venta>()
                .HasMany(v => v.Detalles)
                .WithOne(d => d.Venta)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cotizacion)
                .WithMany()
                .HasForeignKey(v => v.CotizacionId)
                .OnDelete(DeleteBehavior.SetNull);

            // ✅ ÍNDICES PARA MEJORAR RENDIMIENTO
            modelBuilder.Entity<MovimientoStock>()
                .HasIndex(m => new { m.MateriaPrimaId, m.Fecha })
                .HasDatabaseName("IX_MovimientoStock_MateriaPrima_Fecha");

            modelBuilder.Entity<Venta>()
                .HasIndex(v => v.FechaVenta)
                .HasDatabaseName("IX_Venta_FechaVenta");

            modelBuilder.Entity<Venta>()
                .HasIndex(v => v.NumeroVenta)
                .IsUnique()
                .HasDatabaseName("IX_Venta_NumeroVenta");
        }
    }
}
