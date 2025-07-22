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

        public int? VentaId { get; set; }
        public virtual Venta? Venta { get; set; }

        public int Calificacion { get; set; } // 1-5 estrellas

        [Required]
        [StringLength(1000)]
        public string Contenido { get; set; } = string.Empty;

        public DateTime FechaComentario { get; set; } = DateTime.Now;

        public bool Aprobado { get; set; } = false;

        public bool Activo { get; set; } = true;

        [StringLength(500)]
        public string? RespuestaAdmin { get; set; }

        public DateTime? FechaRespuesta { get; set; }
    }
}