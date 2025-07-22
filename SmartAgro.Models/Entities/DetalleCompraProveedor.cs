using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAgro.Models.Entities
{
    public class DetalleCompraProveedor
    {
        public int Id { get; set; }

        // Foreign Keys
        public int CompraProveedorId { get; set; }
        public virtual CompraProveedor CompraProveedor { get; set; } = null!;

        public int MateriaPrimaId { get; set; }
        public virtual MateriaPrima MateriaPrima { get; set; } = null!;

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
    }
}
