using System.ComponentModel.DataAnnotations;

namespace SmartAgro.Models.DTOs.Users
{
    public class UserListDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string NombreCompleto => $"{Nombre} {Apellidos}";
    }
}
