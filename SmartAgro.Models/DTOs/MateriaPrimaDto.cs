// SmartAgro.Models/DTOs/MateriaPrimaDto.cs
using System.ComponentModel.DataAnnotations;

namespace SmartAgro.Models.DTOs
{
    public class MateriaPrimaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal CostoUnitario { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public bool Activo { get; set; }
        public int ProveedorId { get; set; }
        public string? ProveedorNombre { get; set; }
        public decimal ValorInventario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    public class MateriaPrimaCreateDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La unidad de medida es requerida")]
        [StringLength(20, ErrorMessage = "La unidad de medida no puede exceder 20 caracteres")]
        public string UnidadMedida { get; set; } = string.Empty;

        [Required(ErrorMessage = "El costo unitario es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor a 0")]
        public decimal CostoUnitario { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; } = 0;

        [Required(ErrorMessage = "El stock mínimo es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int StockMinimo { get; set; }

        [Required(ErrorMessage = "El proveedor es requerido")]
        public int ProveedorId { get; set; }
    }

    public class MateriaPrimaUpdateDto : MateriaPrimaCreateDto
    {
        public bool Activo { get; set; } = true;
    }

    public class MovimientoStockDto
    {
        [Required]
        public int MateriaPrimaId { get; set; }

        [Required]
        [RegularExpression("^(Entrada|Salida|Ajuste)$", ErrorMessage = "Tipo debe ser: Entrada, Salida o Ajuste")]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor a 0")]
        public decimal CostoUnitario { get; set; }

        [StringLength(100)]
        public string? Referencia { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }
    }

    public class MovimientoStockResponseDto
    {
        public int Id { get; set; }
        public int MateriaPrimaId { get; set; }
        public string MateriaPrimaNombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public DateTime Fecha { get; set; }
        public string? Referencia { get; set; }
        public string? Observaciones { get; set; }
    }
}