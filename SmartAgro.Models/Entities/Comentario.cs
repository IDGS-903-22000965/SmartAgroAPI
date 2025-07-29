using System.ComponentModel.DataAnnotations;

namespace SmartAgro.Models.Entities
{
    public class Comentario
    {
        public int Id { get; set; }

        // Foreign Keys
        public string UsuarioId { get; set; } = string.Empty;
        public virtual Usuario Usuario { get; set; } = null!;

        public int ProductoId { get; set; }
        public virtual Producto Producto { get; set; } = null!;

        [Range(1, 5)]
        public int Calificacion { get; set; } = 5;

        [Required]
        [StringLength(2000)]
        public string Contenido { get; set; } = string.Empty;

        public DateTime FechaComentario { get; set; } = DateTime.Now;

        [StringLength(2000)]
        public string? RespuestaAdmin { get; set; }

        public DateTime? FechaRespuesta { get; set; }

        public bool Aprobado { get; set; } = false;

        public bool Activo { get; set; } = true;
    }
}
