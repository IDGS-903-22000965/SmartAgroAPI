using System.ComponentModel.DataAnnotations;

namespace SmartAgro.Models.DTOs
{
    public class AccountRequestDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 100 caracteres")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Teléfono inválido")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe tener 10 dígitos")]
        public string? Telefono { get; set; }

        [StringLength(200, ErrorMessage = "El nombre de la empresa no puede exceder 200 caracteres")]
        public string? Empresa { get; set; }

        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 500 caracteres")]
        public string Mensaje { get; set; } = string.Empty;

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    }
}