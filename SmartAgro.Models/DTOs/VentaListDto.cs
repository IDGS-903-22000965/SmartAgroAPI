using System.ComponentModel.DataAnnotations;

namespace SmartAgro.Models.DTOs.Ventas
{
    // DTO para listar ventas
    public class VentaListDto
    {
        public int Id { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string? EmailCliente { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaVenta { get; set; }
        public string EstadoVenta { get; set; } = string.Empty;
        public string? MetodoPago { get; set; }
        public int CantidadItems { get; set; }
        public string? NumeroCotizacion { get; set; }
    }

    // DTO para detalles completos de una venta
    public class VentaDetalleDto
    {
        public int Id { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public int? CotizacionId { get; set; }
        public string? NumeroCotizacion { get; set; }

        // Datos del cliente
        public string NombreCliente { get; set; } = string.Empty;
        public string? EmailCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public string? DireccionEntrega { get; set; }

        // Datos financieros
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }

        public DateTime FechaVenta { get; set; }
        public string EstadoVenta { get; set; } = string.Empty;
        public string? MetodoPago { get; set; }
        public string? Observaciones { get; set; }

        public List<DetalleVentaDto> Detalles { get; set; } = new List<DetalleVentaDto>();
    }

    // DTO para los detalles de una venta
    public class DetalleVentaDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? DescripcionProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string? ImagenProducto { get; set; }
    }

    // DTO para crear una nueva venta
    public class CreateVentaDto
    {
        public string UsuarioId { get; set; } = string.Empty; // Se asigna automáticamente

        [Required(ErrorMessage = "El nombre del cliente es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string NombreCliente { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string? EmailCliente { get; set; }

        [Phone(ErrorMessage = "Teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? TelefonoCliente { get; set; }

        [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
        public string? DireccionEntrega { get; set; }

        [Required(ErrorMessage = "El método de pago es requerido")]
        [StringLength(20, ErrorMessage = "El método de pago no puede exceder 20 caracteres")]
        public string MetodoPago { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos un producto")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        public List<CreateDetalleVentaDto> Detalles { get; set; } = new List<CreateDetalleVentaDto>();
    }

    // DTO para crear detalles de venta
    public class CreateDetalleVentaDto
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio unitario es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }
    }

    // DTO para crear venta desde cotización
    public class CreateVentaFromCotizacionDto
    {
        [Required(ErrorMessage = "El método de pago es requerido")]
        [StringLength(20)]
        public string MetodoPago { get; set; } = string.Empty;

        [StringLength(500)]
        public string? DireccionEntrega { get; set; }

        [StringLength(1000)]
        public string? Observaciones { get; set; }
    }

    // DTO para actualizar estado de venta
    public class ActualizarEstadoVentaDto
    {
        [Required(ErrorMessage = "El estado es requerido")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string EstadoVenta { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }
    }

    // DTO para respuesta paginada
    public class PaginatedVentasDto
    {
        public List<VentaListDto> Ventas { get; set; } = new List<VentaListDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    // DTO para estadísticas de ventas
    public class VentaStatsDto
    {
        public int TotalVentas { get; set; }
        public decimal MontoTotalVentas { get; set; }
        public int VentasHoy { get; set; }
        public decimal MontoVentasHoy { get; set; }
        public int VentasEsteMes { get; set; }
        public decimal MontoVentasEsteMes { get; set; }
        public int VentasPendientes { get; set; }
        public int VentasCompletadas { get; set; }
        public decimal PromedioVentaDiaria { get; set; }
        public decimal CrecimientoMesAnterior { get; set; }

        // Distribución por estado
        public Dictionary<string, int> VentasPorEstado { get; set; } = new Dictionary<string, int>();

        // Distribución por método de pago
        public Dictionary<string, int> VentasPorMetodoPago { get; set; } = new Dictionary<string, int>();
    }

    // DTO para reporte de ventas
    public class ReporteVentasDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string TipoAgrupacion { get; set; } = string.Empty;
        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
        public decimal PromedioVenta { get; set; }

        public List<VentaPorPeriodoDto> VentasPorPeriodo { get; set; } = new List<VentaPorPeriodoDto>();
        public List<ProductoVentaDto> ProductosMasVendidos { get; set; } = new List<ProductoVentaDto>();
        public List<ClienteVentaDto> ClientesFrecuentes { get; set; } = new List<ClienteVentaDto>();
    }

    // DTO para ventas agrupadas por período
    public class VentaPorPeriodoDto
    {
        public string Periodo { get; set; } = string.Empty; // "2024-01", "2024-01-15", etc.
        public string PeriodoTexto { get; set; } = string.Empty; // "Enero 2024", "15 Enero 2024", etc.
        public int CantidadVentas { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal PromedioVenta { get; set; }
    }

    // DTO para productos más vendidos
    public class ProductoVentaDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal MontoTotal { get; set; }
        public int NumeroVentas { get; set; }
        public decimal PromedioVenta { get; set; }
        public string? ImagenProducto { get; set; }
    }

    // DTO para clientes frecuentes
    public class ClienteVentaDto
    {
        public string? UsuarioId { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string? EmailCliente { get; set; }
        public int NumeroCompras { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal PromedioCompra { get; set; }
        public DateTime UltimaCompra { get; set; }
    }

    // DTO para ventas por método de pago
    public class VentaPorMetodoPagoDto
    {
        public string MetodoPago { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal Porcentaje { get; set; }
    }

    // DTO para ventas por estado
    public class VentaPorEstadoDto
    {
        public string Estado { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal Porcentaje { get; set; }
    }
}