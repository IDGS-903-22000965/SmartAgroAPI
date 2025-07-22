using System.ComponentModel.DataAnnotations;

namespace SmartAgro.Models.DTOs
{
    public class CotizacionRequestDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string NombreCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string EmailCliente { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Teléfono inválido")]
        public string? TelefonoCliente { get; set; }

        [Required(ErrorMessage = "La dirección de instalación es requerida")]
        public string DireccionInstalacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El área de cultivo es requerida")]
        [Range(1, 10000, ErrorMessage = "El área debe estar entre 1 y 10,000 m²")]
        public decimal AreaCultivo { get; set; }

        [Required(ErrorMessage = "El tipo de cultivo es requerido")]
        public string TipoCultivo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de suelo es requerido")]
        public string TipoSuelo { get; set; } = string.Empty;

        public bool FuenteAguaDisponible { get; set; }

        public bool EnergiaElectricaDisponible { get; set; }

        public string? RequierimientosEspeciales { get; set; }
    }
}