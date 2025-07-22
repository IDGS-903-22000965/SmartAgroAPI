using System.ComponentModel.DataAnnotations;

namespace SmartAgro.Models.DTOs
{
    public class ComentarioDto
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public int Calificacion { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaComentario { get; set; }
        public string? RespuestaAdmin { get; set; }
        public DateTime? FechaRespuesta { get; set; }
    }

    public class ComentarioCreateDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int Calificacion { get; set; }

        [Required(ErrorMessage = "El contenido es requerido")]
        [StringLength(1000, ErrorMessage = "El comentario no puede exceder 1000 caracteres")]
        public string Contenido { get; set; } = string.Empty;
    }

    public class ComentarioRespuestaDto
    {
        [Required(ErrorMessage = "La respuesta es requerida")]
        [StringLength(500, ErrorMessage = "La respuesta no puede exceder 500 caracteres")]
        public string Respuesta { get; set; } = string.Empty;
    }
}