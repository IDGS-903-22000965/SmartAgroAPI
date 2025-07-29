using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAgro.Models.DTOs
{
    public class ActualizarEstadoDto
    {
        [Required(ErrorMessage = "El estado es requerido")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        [JsonPropertyName("estado")]  
        public string Estado { get; set; } = string.Empty;
    }
}