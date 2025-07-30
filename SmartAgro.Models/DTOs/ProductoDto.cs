using System.ComponentModel.DataAnnotations;

namespace SmartAgro.Models.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? DescripcionDetallada { get; set; }

        public decimal PrecioBase { get; set; }
        public decimal PorcentajeGanancia { get; set; }

        public decimal PrecioVenta { get; set; }
        public string? ImagenPrincipal { get; set; }
        public List<string>? ImagenesSecundarias { get; set; }
        public string? VideoDemo { get; set; }
        public List<string>? Caracteristicas { get; set; }
        public List<string>? Beneficios { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<ComentarioDto>? Comentarios { get; set; }
    }

    public class ProductoCreateDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [StringLength(2000)]
        public string? DescripcionDetallada { get; set; }

        [Required(ErrorMessage = "El precio base es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioBase { get; set; }

        [Required(ErrorMessage = "El porcentaje de ganancia es requerido")]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        public decimal PorcentajeGanancia { get; set; }

        public string? ImagenPrincipal { get; set; }

        public List<string>? ImagenesSecundarias { get; set; }

        [StringLength(1000)]
        public string? VideoDemo { get; set; }

        public List<string>? Caracteristicas { get; set; }
        public List<string>? Beneficios { get; set; }
    }

    public class ProductoUpdateDto : ProductoCreateDto
    {
        public bool Activo { get; set; } = true;
    }

    public class ProductoDetalleDto : ProductoDto
    {
        public List<ProductoMateriaPrimaDto> MateriasPrimas { get; set; } = new List<ProductoMateriaPrimaDto>();
        public double PromedioCalificacion { get; set; }
        public int TotalComentarios { get; set; }
    }

    public class ProductoMateriaPrimaDto
    {
        public int Id { get; set; }
        public string NombreMateriaPrima { get; set; } = string.Empty;
        public decimal CantidadRequerida { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal CostoUnitario { get; set; }
        public decimal CostoTotal { get; set; }
        public string? Notas { get; set; }
    }

    public class ProductoMateriaPrimaDetalleDto : ProductoMateriaPrimaDto
    {
        public int MateriaPrimaId { get; set; }
        public string? DescripcionMateriaPrima { get; set; }
        public int StockDisponible { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;
    }

    public class ProductoMateriaPrimaCreateDto
    {
        [Required]
        public int MateriaPrimaId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal CantidadRequerida { get; set; }

        [StringLength(200)]
        public string? Notas { get; set; }
    }
}