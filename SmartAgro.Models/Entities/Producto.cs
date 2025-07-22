using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAgro.Models.Entities
{
    public class Producto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [StringLength(2000)]
        public string? DescripcionDetallada { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioBase { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeGanancia { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }

        [StringLength(500)]
        public string? ImagenPrincipal { get; set; }

        [StringLength(2000)]
        public string? ImagenesSecundarias { get; set; } // JSON array de URLs

        [StringLength(1000)]
        public string? VideoDemo { get; set; }

        [StringLength(2000)]
        public string? Caracteristicas { get; set; } // JSON array

        [StringLength(1000)]
        public string? Beneficios { get; set; } // JSON array

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relaciones
        public virtual ICollection<ProductoMateriaPrima> ProductoMateriasPrimas { get; set; } = new List<ProductoMateriaPrima>();
        public virtual ICollection<DetalleCotizacion> DetallesCotizacion { get; set; } = new List<DetalleCotizacion>();
        public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
        public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    }
}