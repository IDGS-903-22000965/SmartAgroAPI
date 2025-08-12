// SmartAgro.Models/Entities/ProductoDocumento.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAgro.Models.Entities
{
    public class ProductoDocumento
    {
        public int Id { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty; // PDF, Video, Link, Manual, Guia, etc.

        [Required]
        [StringLength(1000)]
        public string Url { get; set; } = string.Empty;

        public bool EsVisibleParaCliente { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relación con Producto
        public virtual Producto Producto { get; set; } = null!;
    }
}