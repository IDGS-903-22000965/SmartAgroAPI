using System.ComponentModel.DataAnnotations;

namespace SmartAgro.Models.Entities
{
    public class Proveedor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string RazonSocial { get; set; } = string.Empty;

        [StringLength(20)]
        public string? RFC { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? ContactoPrincipal { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Relaciones
        public virtual ICollection<MateriaPrima> MateriasPrimas { get; set; } = new List<MateriaPrima>();
        public virtual ICollection<CompraProveedor> Compras { get; set; } = new List<CompraProveedor>();
    }
}