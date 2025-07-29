using System.ComponentModel.DataAnnotations;

namespace SmartAgro.Models.DTOs.ComprasProveedores
{
    // DTO para listar compras (vista principal)
    public class CompraProveedorListDto
    {
        public int Id { get; set; }
        public string NumeroCompra { get; set; } = string.Empty;
        public string ProveedorNombre { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime FechaCompra { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int CantidadItems { get; set; }
        public string? Observaciones { get; set; }
    }

    // DTO para detalles completos de una compra
    public class CompraProveedorDetailsDto
    {
        public int Id { get; set; }
        public string NumeroCompra { get; set; } = string.Empty;
        public int ProveedorId { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;
        public string ProveedorRazonSocial { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime FechaCompra { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public List<DetalleCompraDto> Detalles { get; set; } = new List<DetalleCompraDto>();
    }

    // DTO para los detalles de una compra
    public class DetalleCompraDto
    {
        public int Id { get; set; }
        public int MateriaPrimaId { get; set; }
        public string MateriaPrimaNombre { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string? Notas { get; set; }
    }

    // DTO para crear una nueva compra
    public class CreateCompraProveedorDto
    {
        [Required(ErrorMessage = "El proveedor es requerido")]
        public int ProveedorId { get; set; }

        [Required(ErrorMessage = "La fecha de compra es requerida")]
        public DateTime FechaCompra { get; set; }

        [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos un detalle")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un detalle")]
        public List<CreateDetalleCompraDto> Detalles { get; set; } = new List<CreateDetalleCompraDto>();
    }

    // DTO para actualizar una compra existente
    public class UpdateCompraProveedorDto
    {
        [Required(ErrorMessage = "El proveedor es requerido")]
        public int ProveedorId { get; set; }

        [Required(ErrorMessage = "La fecha de compra es requerida")]
        public DateTime FechaCompra { get; set; }

        [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos un detalle")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un detalle")]
        public List<CreateDetalleCompraDto> Detalles { get; set; } = new List<CreateDetalleCompraDto>();
    }

    // DTO para crear/actualizar detalles de compra
    public class CreateDetalleCompraDto
    {
        [Required(ErrorMessage = "La materia prima es requerida")]
        public int MateriaPrimaId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        [Required(ErrorMessage = "El precio unitario es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }

        [StringLength(200, ErrorMessage = "Las notas no pueden exceder 200 caracteres")]
        public string? Notas { get; set; }
    }

    // DTO para respuesta paginada
    public class PaginatedComprasDto
    {
        public List<CompraProveedorListDto> Compras { get; set; } = new List<CompraProveedorListDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    // DTO para estadísticas
    public class CompraStatsDto
    {
        public int TotalCompras { get; set; }
        public int ComprasPendientes { get; set; }
        public int ComprasRecibidas { get; set; }
        public int ComprasCanceladas { get; set; }
        public int ComprasEsteMes { get; set; }
        public decimal TotalGastado { get; set; }
        public decimal GastoEsteMes { get; set; }
    }
}
