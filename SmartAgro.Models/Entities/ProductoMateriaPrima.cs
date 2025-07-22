using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAgro.Models.Entities
{
    public class ProductoMateriaPrima
    {
        public int Id { get; set; }

        // Foreign Keys
        public int ProductoId { get; set; }
        public virtual Producto Producto { get; set; } = null!;

        public int MateriaPrimaId { get; set; }
        public virtual MateriaPrima MateriaPrima { get; set; } = null!;

        [Column(TypeName = "decimal(18,4)")]
        public decimal CantidadRequerida { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoTotal { get; set; }

        [StringLength(200)]
        public string? Notas { get; set; }
    }
}
