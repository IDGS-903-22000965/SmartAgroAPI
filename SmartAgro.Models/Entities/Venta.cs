using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAgro.Models.Entities
{
    public class Venta
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string NumeroVenta { get; set; } = string.Empty;

        // Foreign Keys
        public string UsuarioId { get; set; } = string.Empty;
        public virtual Usuario Usuario { get; set; } = null!;

        public int? CotizacionId { get; set; }
        public virtual Cotizacion? Cotizacion { get; set; }

        // ✅ PROPIEDADES FINANCIERAS
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Impuestos { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public DateTime FechaVenta { get; set; } = DateTime.Now;

        // ✅ CAMBIAR Estado por EstadoVenta
        [StringLength(20)]
        public string EstadoVenta { get; set; } = "Pendiente"; // Pendiente, Procesando, Enviado, Entregado, Cancelado

        [StringLength(20)]
        public string? MetodoPago { get; set; }

        [StringLength(1000)]
        public string? Observaciones { get; set; }

        // ✅ DATOS DEL CLIENTE (no están en las entidades originales, los agregamos)
        [Required]
        [StringLength(100)]
        public string NombreCliente { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EmailCliente { get; set; }

        [StringLength(20)]
        public string? TelefonoCliente { get; set; }

        [StringLength(500)]
        public string? DireccionEntrega { get; set; }

        // Relaciones
        public virtual ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}