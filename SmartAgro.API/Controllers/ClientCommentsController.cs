// SmartAgro.API/Controllers/ClientCommentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.DTOs;
using SmartAgro.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/client/comments")]
    [Authorize(Roles = "Cliente")]
    public class ClientCommentsController : ControllerBase
    {
        private readonly SmartAgroDbContext _context;
        private readonly ILogger<ClientCommentsController> _logger;

        public ClientCommentsController(
            SmartAgroDbContext context,
            ILogger<ClientCommentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los comentarios del cliente actual
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ClientCommentDto>>> GetMyComments()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var comments = await _context.Comentarios
                    .Where(c => c.UsuarioId == currentUserId)
                    .Include(c => c.Producto)
                    .OrderByDescending(c => c.FechaComentario)
                    .Select(c => new ClientCommentDto
                    {
                        Id = c.Id,
                        ProductoId = c.ProductoId,
                        NombreProducto = c.Producto.Nombre,
                        ImagenProducto = c.Producto.ImagenPrincipal,
                        Calificacion = c.Calificacion,
                        Contenido = c.Contenido,
                        FechaComentario = c.FechaComentario,
                        Aprobado = c.Aprobado,
                        Activo = c.Activo,
                        RespuestaAdmin = c.RespuestaAdmin,
                        FechaRespuesta = c.FechaRespuesta,
                        EstadoTexto = c.Aprobado ? "Aprobado" : (c.Activo ? "Pendiente" : "Rechazado")
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = comments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comentarios del cliente");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener comentarios"
                });
            }
        }

        /// <summary>
        /// Crea un nuevo comentario para un producto comprado
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateClientCommentDto createCommentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                // Verificar que el producto existe
                var producto = await _context.Productos.FindAsync(createCommentDto.ProductoId);
                if (producto == null)
                    return NotFound(new
                    {
                        success = false,
                        message = "Producto no encontrado"
                    });

                // Verificar que el cliente haya comprado este producto
                var hasPurchased = await _context.DetallesVenta
                    .AnyAsync(d => d.Producto.Id == createCommentDto.ProductoId &&
                                  d.Venta.UsuarioId == currentUserId);

                if (!hasPurchased)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Solo puedes comentar productos que hayas comprado"
                    });

                // Verificar que no haya comentado ya este producto
                var existingComment = await _context.Comentarios
                    .FirstOrDefaultAsync(c => c.UsuarioId == currentUserId &&
                                             c.ProductoId == createCommentDto.ProductoId);

                if (existingComment != null)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Ya has comentado este producto"
                    });

                var comentario = new Comentario
                {
                    UsuarioId = currentUserId,
                    ProductoId = createCommentDto.ProductoId,
                    Calificacion = createCommentDto.Calificacion,
                    Contenido = createCommentDto.Contenido,
                    FechaComentario = DateTime.Now,
                    Aprobado = false, // Los comentarios requieren aprobación
                    Activo = true
                };

                _context.Comentarios.Add(comentario);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Comentario creado por cliente {currentUserId} para producto {createCommentDto.ProductoId}");

                return Ok(new
                {
                    success = true,
                    message = "Comentario enviado exitosamente. Será revisado antes de publicarse.",
                    data = new { Id = comentario.Id }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear comentario");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear comentario"
                });
            }
        }

        /// <summary>
        /// Actualiza un comentario existente (solo si no ha sido aprobado)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateClientCommentDto updateCommentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var comentario = await _context.Comentarios
                    .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == currentUserId);

                if (comentario == null)
                    return NotFound(new
                    {
                        success = false,
                        message = "Comentario no encontrado"
                    });

                // Solo se puede editar si no ha sido aprobado
                if (comentario.Aprobado)
                    return BadRequest(new
                    {
                        success = false,
                        message = "No puedes editar un comentario que ya ha sido aprobado"
                    });

                comentario.Calificacion = updateCommentDto.Calificacion;
                comentario.Contenido = updateCommentDto.Contenido;
                comentario.FechaComentario = DateTime.Now; // Actualizar fecha

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Comentario {id} actualizado por cliente {currentUserId}");

                return Ok(new
                {
                    success = true,
                    message = "Comentario actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar comentario {CommentId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al actualizar comentario"
                });
            }
        }

        /// <summary>
        /// Elimina un comentario (solo si no ha sido aprobado)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var comentario = await _context.Comentarios
                    .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == currentUserId);

                if (comentario == null)
                    return NotFound(new
                    {
                        success = false,
                        message = "Comentario no encontrado"
                    });

                // Solo se puede eliminar si no ha sido aprobado
                if (comentario.Aprobado)
                    return BadRequest(new
                    {
                        success = false,
                        message = "No puedes eliminar un comentario que ya ha sido aprobado"
                    });

                // Soft delete
                comentario.Activo = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Comentario {id} eliminado por cliente {currentUserId}");

                return Ok(new
                {
                    success = true,
                    message = "Comentario eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar comentario {CommentId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al eliminar comentario"
                });
            }
        }

        /// <summary>
        /// Obtiene los productos que el cliente puede comentar
        /// </summary>
        [HttpGet("available-products")]
        public async Task<ActionResult<List<CommentableProductDto>>> GetCommentableProducts()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                // Obtener productos comprados que aún no han sido comentados
                var commentableProducts = await _context.DetallesVenta
                    .Where(d => d.Venta.UsuarioId == currentUserId)
                    .Include(d => d.Producto)
                    .GroupBy(d => d.ProductoId)
                    .Where(g => !_context.Comentarios.Any(c =>
                        c.UsuarioId == currentUserId &&
                        c.ProductoId == g.Key))
                    .Select(g => new CommentableProductDto
                    {
                        ProductoId = g.Key,
                        NombreProducto = g.First().Producto.Nombre,
                        DescripcionProducto = g.First().Producto.Descripcion,
                        ImagenProducto = g.First().Producto.ImagenPrincipal,
                        FechaUltimaCompra = g.Max(d => d.Venta.FechaVenta),
                        TotalComprado = g.Sum(d => d.Cantidad),
                        NumeroCompras = g.Select(d => d.VentaId).Distinct().Count()
                    })
                    .OrderByDescending(p => p.FechaUltimaCompra)
                    .ToListAsync();

                return Ok(new { success = true, data = commentableProducts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos comentables");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener productos"
                });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de comentarios del cliente
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ClientCommentStatsDto>> GetCommentStats()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var totalComentarios = await _context.Comentarios
                    .CountAsync(c => c.UsuarioId == currentUserId);

                var comentariosAprobados = await _context.Comentarios
                    .CountAsync(c => c.UsuarioId == currentUserId && c.Aprobado);

                var comentariosPendientes = await _context.Comentarios
                    .CountAsync(c => c.UsuarioId == currentUserId && !c.Aprobado && c.Activo);

                var comentariosRechazados = await _context.Comentarios
                    .CountAsync(c => c.UsuarioId == currentUserId && !c.Activo);

                var calificacionPromedio = await _context.Comentarios
                    .Where(c => c.UsuarioId == currentUserId)
                    .AverageAsync(c => (double?)c.Calificacion) ?? 0;

                var stats = new ClientCommentStatsDto
                {
                    TotalComentarios = totalComentarios,
                    ComentariosAprobados = comentariosAprobados,
                    ComentariosPendientes = comentariosPendientes,
                    ComentariosRechazados = comentariosRechazados,
                    CalificacionPromedio = Math.Round(calificacionPromedio, 1)
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de comentarios");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener estadísticas"
                });
            }
        }
    }
}

// DTOs para comentarios del cliente
namespace SmartAgro.Models.DTOs
{
    public class ClientCommentDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? ImagenProducto { get; set; }
        public int Calificacion { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaComentario { get; set; }
        public bool Aprobado { get; set; }
        public bool Activo { get; set; }
        public string? RespuestaAdmin { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public string EstadoTexto { get; set; } = string.Empty;
    }

    public class CreateClientCommentDto
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La calificación es requerida")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int Calificacion { get; set; }

        [Required(ErrorMessage = "El contenido es requerido")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "El comentario debe tener entre 10 y 1000 caracteres")]
        public string Contenido { get; set; } = string.Empty;
    }

    public class UpdateClientCommentDto
    {
        [Required(ErrorMessage = "La calificación es requerida")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int Calificacion { get; set; }

        [Required(ErrorMessage = "El contenido es requerido")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "El comentario debe tener entre 10 y 1000 caracteres")]
        public string Contenido { get; set; } = string.Empty;
    }

    public class CommentableProductDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? DescripcionProducto { get; set; }
        public string? ImagenProducto { get; set; }
        public DateTime FechaUltimaCompra { get; set; }
        public int TotalComprado { get; set; }
        public int NumeroCompras { get; set; }
    }

    public class ClientCommentStatsDto
    {
        public int TotalComentarios { get; set; }
        public int ComentariosAprobados { get; set; }
        public int ComentariosPendientes { get; set; }
        public int ComentariosRechazados { get; set; }
        public double CalificacionPromedio { get; set; }
    }
}