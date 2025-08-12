using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAgro.Models.Entities
{
    public class Cotizacion
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string NumeroCotizacion { get; set; } = string.Empty;

        // Foreign Key (nullable para permitir SetNull)
        public string? UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }

        // ✅ DATOS DEL CLIENTE
        [Required]
        [StringLength(100)]
        public string NombreCliente { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EmailCliente { get; set; } = string.Empty;

        [StringLength(20)]
        public string? TelefonoCliente { get; set; }

        // ✅ DATOS TÉCNICOS DEL PROYECTO
        [StringLength(200)]
        public string? DireccionInstalacion { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal AreaCultivo { get; set; }

        [StringLength(50)]
        public string? TipoCultivo { get; set; }

        [StringLength(50)]
        public string? TipoSuelo { get; set; }

        public bool FuenteAguaDisponible { get; set; }

        public bool EnergiaElectricaDisponible { get; set; }


        [StringLength(1000)]
        public string? RequierimientosEspeciales { get; set; }

        // ✅ DATOS FINANCIEROS
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeImpuesto { get; set; } = 16.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Impuestos { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // ✅ FECHAS Y ESTADO
        public DateTime FechaCotizacion { get; set; } = DateTime.Now;

        public DateTime FechaVencimiento { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobada, Rechazada, Vencida

        [StringLength(1000)]
        public string? Observaciones { get; set; }

        // Relaciones
        public virtual ICollection<DetalleCotizacion> Detalles { get; set; } = new List<DetalleCotizacion>();
    }
}