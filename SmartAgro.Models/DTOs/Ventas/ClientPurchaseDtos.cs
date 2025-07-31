// SmartAgro.Models/DTOs/Ventas/ClientPurchaseDtos.cs
namespace SmartAgro.Models.DTOs.Ventas
{
    public class ClientPurchaseDto
    {
        public int Id { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public string EstadoVenta { get; set; } = string.Empty;
        public string? MetodoPago { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string? DireccionEntrega { get; set; }
        public string? Observaciones { get; set; }
        public string? NumeroCotizacion { get; set; }
        public int CantidadProductos { get; set; }
        public List<ClientPurchaseProductDto> Productos { get; set; } = new List<ClientPurchaseProductDto>();
    }

    public class ClientPurchaseProductDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? DescripcionProducto { get; set; }
        public string? ImagenProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class ClientPurchaseDetailDto
    {
        public int Id { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public string EstadoVenta { get; set; } = string.Empty;
        public string? MetodoPago { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string? DireccionEntrega { get; set; }
        public string? Observaciones { get; set; }
        public string? NumeroCotizacion { get; set; }

        // Datos del cliente
        public string NombreCliente { get; set; } = string.Empty;
        public string? EmailCliente { get; set; }
        public string? TelefonoCliente { get; set; }

        public List<ClientPurchaseProductDetailDto> Productos { get; set; } = new List<ClientPurchaseProductDetailDto>();
    }

    public class ClientPurchaseProductDetailDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? DescripcionProducto { get; set; }
        public string? DescripcionDetallada { get; set; }
        public string? ImagenPrincipal { get; set; }
        public List<string>? ImagenesSecundarias { get; set; }
        public string? VideoDemo { get; set; }
        public List<string>? Caracteristicas { get; set; }
        public List<string>? Beneficios { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public bool YaComento { get; set; }
    }

    public class ClientOwnedProductDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? DescripcionProducto { get; set; }
        public string? DescripcionDetallada { get; set; }
        public string? ImagenPrincipal { get; set; }
        public List<string>? ImagenesSecundarias { get; set; }
        public string? VideoDemo { get; set; }
        public List<string>? Caracteristicas { get; set; }
        public List<string>? Beneficios { get; set; }

        // Estadísticas de compra
        public int TotalComprado { get; set; }
        public decimal TotalGastado { get; set; }
        public int NumeroCompras { get; set; }
        public DateTime PrimeraCompra { get; set; }
        public DateTime UltimaCompra { get; set; }
        public bool YaComento { get; set; }
        public double CalificacionPromedio { get; set; }
        public int TotalComentarios { get; set; }
    }

    public class ClientPurchaseStatsDto
    {
        public int TotalCompras { get; set; }
        public decimal TotalGastado { get; set; }
        public int ComprasEsteMes { get; set; }
        public decimal GastoEsteMes { get; set; }
        public decimal PromedioGasto { get; set; }
        public DateTime? PrimeraCompra { get; set; }
        public DateTime? UltimaCompra { get; set; }
        public List<string> ProductosFavoritos { get; set; } = new List<string>();
    }
}