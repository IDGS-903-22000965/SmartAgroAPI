using System.ComponentModel.DataAnnotations;

namespace SmartAgro.Models.DTOs
{
    public class CotizacionResponseDto
    {
        public int Id { get; set; }
        public string NumeroCotizacion { get; set; } = string.Empty;
        public string? UsuarioId { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;
        public string? TelefonoCliente { get; set; }
        public string? DireccionInstalacion { get; set; }
        public decimal AreaCultivo { get; set; }
        public string TipoCultivo { get; set; } = string.Empty;
        public string TipoSuelo { get; set; } = string.Empty;
        public bool FuenteAguaDisponible { get; set; }
        public bool EnergiaElectricaDisponible { get; set; }
        public string? RequierimientosEspeciales { get; set; }
        public decimal Subtotal { get; set; }
        public decimal PorcentajeImpuesto { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaCotizacion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public List<DetalleCotizacionResponseDto> Detalles { get; set; } = new();
    }

    public class DetalleCotizacionResponseDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string? NombreProducto { get; set; } // Info adicional sin referencia circular
    }
}