using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAgro.Models.Entities
{
    public class CompraProveedor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string NumeroCompra { get; set; } = string.Empty;

        // Foreign Key
        public int ProveedorId { get; set; }
        public virtual Proveedor Proveedor { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public DateTime FechaCompra { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Recibido, Cancelado

        [StringLength(1000)]
        public string? Observaciones { get; set; }

        // Relaciones
        public virtual ICollection<DetalleCompraProveedor> Detalles { get; set; } = new List<DetalleCompraProveedor>();
    }
}