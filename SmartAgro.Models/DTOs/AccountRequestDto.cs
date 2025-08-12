using System.ComponentModel.DataAnnotations;

namespace SmartAgro.Models.DTOs
{
    public class AccountRequestDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100, MinimumLength = 2)]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [RegularExpression(@"^\d{10}$")]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Empresa { get; set; }

        [StringLength(500, MinimumLength = 10)]
        public string? Mensaje { get; set; } 

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    }
}