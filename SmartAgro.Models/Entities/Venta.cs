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

        // Foreign Key
        public string UsuarioId { get; set; } = string.Empty;
        public virtual Usuario Usuario { get; set; } = null!;

        public int? CotizacionId { get; set; }
        public virtual Cotizacion? Cotizacion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Impuestos { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public DateTime FechaVenta { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string EstadoVenta { get; set; } = "Pendiente"; // Pendiente, Procesando, Enviado, Entregado, Cancelado

        [StringLength(20)]
        public string MetodoPago { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Observaciones { get; set; }

        // Relaciones
        public virtual ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}