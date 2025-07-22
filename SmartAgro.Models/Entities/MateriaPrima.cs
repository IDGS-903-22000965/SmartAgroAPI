using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAgro.Models.Entities
{
    public class MateriaPrima
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [StringLength(20)]
        public string UnidadMedida { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoUnitario { get; set; }

        public int Stock { get; set; }

        public int StockMinimo { get; set; }

        public bool Activo { get; set; } = true;

        // Foreign Key
        public int ProveedorId { get; set; }
        public virtual Proveedor Proveedor { get; set; } = null!;

        // Relaciones
        public virtual ICollection<ProductoMateriaPrima> ProductoMateriasPrimas { get; set; } = new List<ProductoMateriaPrima>();
        public virtual ICollection<DetalleCompraProveedor> DetallesCompra { get; set; } = new List<DetalleCompraProveedor>();
    }
}