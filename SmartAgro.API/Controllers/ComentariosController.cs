// SmartAgro.API/Controllers/ComentariosController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.DTOs;
using SmartAgro.API.Services;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/comentarios")]
    [Authorize(Roles = "Admin")]
    public class ComentariosController : ControllerBase
    {
        private readonly SmartAgroDbContext _context;
        private readonly IComentarioService _comentarioService;
        private readonly ILogger<ComentariosController> _logger;

        public ComentariosController(
            SmartAgroDbContext context,
            IComentarioService comentarioService,
            ILogger<ComentariosController> logger)
        {
            _context = context;
            _comentarioService = comentarioService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los comentarios para administración
        /// </summary>
        [HttpGet("admin/comentarios")]
        public async Task<IActionResult> GetAllComments()
        {
            try
            {
                var comentarios = await _comentarioService.ObtenerComentariosAsync();

                var response = comentarios.Select(c => new
                {
                    id = c.Id,
                    nombreUsuario = c.Usuario?.Nombre ?? "Usuario",
                    emailUsuario = c.Usuario?.Email ?? "",
                    nombreProducto = c.Producto?.Nombre ?? "Producto",
                    productoId = c.ProductoId,
                    calificacion = c.Calificacion,
                    contenido = c.Contenido,
                    fechaCreacion = c.FechaComentario,
                    aprobado = c.Aprobado,
                    activo = c.Activo,
                    respuestaAdmin = c.RespuestaAdmin,
                    fechaRespuesta = c.FechaRespuesta
                }).ToList();

                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comentarios para admin");
                return StatusCode(500, new { success = false, message = "Error al obtener comentarios" });
            }
        }

        /// <summary>
        /// Obtiene comentarios pendientes de aprobación
        /// </summary>
        [HttpGet("admin/comentarios/pendientes")]
        public async Task<IActionResult> GetPendingComments()
        {
            try
            {
                var comentarios = await _comentarioService.ObtenerComentariosPendientesAsync();

                var response = comentarios.Select(c => new
                {
                    id = c.Id,
                    nombreUsuario = c.Usuario?.Nombre ?? "Usuario",
                    nombreProducto = c.Producto?.Nombre ?? "Producto",
                    calificacion = c.Calificacion,
                    contenido = c.Contenido,
                    fechaCreacion = c.FechaComentario
                }).ToList();

                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comentarios pendientes");
                return StatusCode(500, new { success = false, message = "Error al obtener comentarios pendientes" });
            }
        }

        /// <summary>
        /// Aprobar un comentario
        /// </summary>
        [HttpPut("admin/comentarios/{id}/aprobar")]
        public async Task<IActionResult> AprobarComentario(int id)
        {
            try
            {
                var resultado = await _comentarioService.AprobarComentarioAsync(id);

                if (resultado)
                {
                    _logger.LogInformation($"✅ Comentario {id} aprobado por admin");
                    return Ok(new { success = true, message = "Comentario aprobado exitosamente" });
                }

                return NotFound(new { success = false, message = "Comentario no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aprobar comentario {CommentId}", id);
                return StatusCode(500, new { success = false, message = "Error al aprobar comentario" });
            }
        }

        /// <summary>
        /// Rechazar un comentario
        /// </summary>
        [HttpPut("admin/comentarios/{id}/rechazar")]
        public async Task<IActionResult> RechazarComentario(int id)
        {
            try
            {
                var resultado = await _comentarioService.RechazarComentarioAsync(id);

                if (resultado)
                {
                    _logger.LogInformation($"❌ Comentario {id} rechazado por admin");
                    return Ok(new { success = true, message = "Comentario rechazado" });
                }

                return NotFound(new { success = false, message = "Comentario no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al rechazar comentario {CommentId}", id);
                return StatusCode(500, new { success = false, message = "Error al rechazar comentario" });
            }
        }

        /// <summary>
        /// Responder a un comentario
        /// </summary>
        [HttpPut("admin/comentarios/{id}/responder")]
        public async Task<IActionResult> ResponderComentario(int id, [FromBody] ComentarioRespuestaDto respuestaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var resultado = await _comentarioService.ResponderComentarioAsync(id, respuestaDto.Respuesta);

                if (resultado)
                {
                    _logger.LogInformation($"💬 Respuesta agregada al comentario {id} por admin");
                    return Ok(new { success = true, message = "Respuesta enviada exitosamente" });
                }

                return NotFound(new { success = false, message = "Comentario no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al responder comentario {CommentId}", id);
                return StatusCode(500, new { success = false, message = "Error al enviar respuesta" });
            }
        }

        /// <summary>
        /// Obtiene comentarios públicos aprobados (para testimonios)
        /// </summary>
        [HttpGet("publicos")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicComments()
        {
            try
            {
                var comentarios = await _context.Comentarios
                    .Where(c => c.Aprobado && c.Activo)
                    .Include(c => c.Usuario)
                    .Include(c => c.Producto)
                    .OrderByDescending(c => c.FechaComentario)
                    .Select(c => new
                    {
                        id = c.Id,
                        nombreUsuario = c.Usuario.Nombre ?? "Cliente SmartAgro",
                        nombreProducto = c.Producto.Nombre,
                        calificacion = c.Calificacion,
                        contenido = c.Contenido,
                        fechaCreacion = c.FechaComentario,
                        respuestaAdmin = c.RespuestaAdmin,
                        aprobado = c.Aprobado,
                        activo = c.Activo
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = comentarios });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comentarios públicos");
                return StatusCode(500, new { success = false, message = "Error al obtener comentarios públicos" });
            }
        }

        /// <summary>
        /// Obtiene comentarios de un producto específico (público)
        /// </summary>
        [HttpGet("producto/{productoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductComments(int productoId)
        {
            try
            {
                var comentarios = await _comentarioService.ObtenerComentariosPorProductoAsync(productoId);

                var response = comentarios.Select(c => new ComentarioDto
                {
                    Id = c.Id,
                    NombreUsuario = c.Usuario?.Nombre ?? "Usuario",
                    Calificacion = c.Calificacion,
                    Contenido = c.Contenido,
                    FechaComentario = c.FechaComentario,
                    RespuestaAdmin = c.RespuestaAdmin,
                    FechaRespuesta = c.FechaRespuesta
                }).ToList();

                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comentarios del producto {ProductId}", productoId);
                return StatusCode(500, new { success = false, message = "Error al obtener comentarios" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de comentarios para el dashboard admin
        /// </summary>
        [HttpGet("admin/stats")]
        public async Task<IActionResult> GetCommentStats()
        {
            try
            {
                var totalComentarios = await _context.Comentarios.CountAsync();
                var comentariosPendientes = await _context.Comentarios.CountAsync(c => !c.Aprobado && c.Activo);
                var comentariosAprobados = await _context.Comentarios.CountAsync(c => c.Aprobado);
                var comentariosRechazados = await _context.Comentarios.CountAsync(c => !c.Activo);
                var promedioCalificacion = await _context.Comentarios
                    .Where(c => c.Aprobado)
                    .AverageAsync(c => (double?)c.Calificacion) ?? 0;

                var stats = new
                {
                    totalComentarios,
                    comentariosPendientes,
                    comentariosAprobados,
                    comentariosRechazados,
                    promedioCalificacion = Math.Round(promedioCalificacion, 1)
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de comentarios");
                return StatusCode(500, new { success = false, message = "Error al obtener estadísticas" });
            }
        }
    }
}