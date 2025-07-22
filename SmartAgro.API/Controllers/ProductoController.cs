// SmartAgro.API/Controllers/ProductoController.cs - Versión corregida
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.DTOs;
using SmartAgro.Models.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductoController : ControllerBase
    {
        private readonly SmartAgroDbContext _context;

        public ProductoController(SmartAgroDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProductos()
        {
            try
            {
                // Primero obtenemos los datos sin deserializar
                var productosDb = await _context.Productos
                    .Where(p => p.Activo)
                    .Include(p => p.Comentarios.Where(c => c.Aprobado && c.Activo))
                    .ThenInclude(c => c.Usuario)
                    .ToListAsync();

                // Luego mapeamos a DTOs en memoria
                var productos = productosDb.Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    DescripcionDetallada = p.DescripcionDetallada,
                    PrecioVenta = p.PrecioVenta,
                    ImagenPrincipal = p.ImagenPrincipal,
                    ImagenesSecundarias = DeserializeStringList(p.ImagenesSecundarias),
                    VideoDemo = p.VideoDemo,
                    Caracteristicas = DeserializeStringList(p.Caracteristicas),
                    Beneficios = DeserializeStringList(p.Beneficios),
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    Comentarios = p.Comentarios.Select(c => new ComentarioDto
                    {
                        Id = c.Id,
                        NombreUsuario = $"{c.Usuario.Nombre} {c.Usuario.Apellidos}",
                        Calificacion = c.Calificacion,
                        Contenido = c.Contenido,
                        FechaComentario = c.FechaComentario,
                        RespuestaAdmin = c.RespuestaAdmin,
                        FechaRespuesta = c.FechaRespuesta
                    }).ToList()
                }).ToList();

                return Ok(new { success = true, data = productos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener los productos",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerProductoPorId(int id)
        {
            try
            {
                // Primero obtenemos los datos de la base
                var productoDb = await _context.Productos
                    .Where(p => p.Id == id && p.Activo)
                    .Include(p => p.Comentarios.Where(c => c.Aprobado && c.Activo))
                    .ThenInclude(c => c.Usuario)
                    .Include(p => p.ProductoMateriasPrimas)
                    .ThenInclude(pm => pm.MateriaPrima)
                    .FirstOrDefaultAsync();

                if (productoDb == null)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                // Luego mapeamos a DTO en memoria
                var producto = new ProductoDetalleDto
                {
                    Id = productoDb.Id,
                    Nombre = productoDb.Nombre,
                    Descripcion = productoDb.Descripcion,
                    DescripcionDetallada = productoDb.DescripcionDetallada,
                    PrecioVenta = productoDb.PrecioVenta,
                    ImagenPrincipal = productoDb.ImagenPrincipal,
                    ImagenesSecundarias = DeserializeStringList(productoDb.ImagenesSecundarias),
                    VideoDemo = productoDb.VideoDemo,
                    Caracteristicas = DeserializeStringList(productoDb.Caracteristicas),
                    Beneficios = DeserializeStringList(productoDb.Beneficios),
                    Activo = productoDb.Activo,
                    FechaCreacion = productoDb.FechaCreacion,
                    MateriasPrimas = productoDb.ProductoMateriasPrimas.Select(pm => new ProductoMateriaPrimaDto
                    {
                        Id = pm.Id,
                        NombreMateriaPrima = pm.MateriaPrima.Nombre,
                        CantidadRequerida = pm.CantidadRequerida,
                        UnidadMedida = pm.MateriaPrima.UnidadMedida,
                        CostoUnitario = pm.CostoUnitario,
                        CostoTotal = pm.CostoTotal,
                        Notas = pm.Notas
                    }).ToList(),
                    Comentarios = productoDb.Comentarios.Select(c => new ComentarioDto
                    {
                        Id = c.Id,
                        NombreUsuario = $"{c.Usuario.Nombre} {c.Usuario.Apellidos}",
                        Calificacion = c.Calificacion,
                        Contenido = c.Contenido,
                        FechaComentario = c.FechaComentario,
                        RespuestaAdmin = c.RespuestaAdmin,
                        FechaRespuesta = c.FechaRespuesta
                    }).ToList(),
                    PromedioCalificacion = productoDb.Comentarios.Any() ? productoDb.Comentarios.Average(c => c.Calificacion) : 0,
                    TotalComentarios = productoDb.Comentarios.Count()
                };

                return Ok(new { success = true, data = producto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener el producto",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CrearProducto([FromBody] ProductoCreateDto productoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var producto = new Producto
                {
                    Nombre = productoDto.Nombre,
                    Descripcion = productoDto.Descripcion,
                    DescripcionDetallada = productoDto.DescripcionDetallada,
                    PrecioBase = productoDto.PrecioBase,
                    PorcentajeGanancia = productoDto.PorcentajeGanancia,
                    PrecioVenta = productoDto.PrecioBase * (1 + productoDto.PorcentajeGanancia / 100),
                    ImagenPrincipal = productoDto.ImagenPrincipal,
                    ImagenesSecundarias = SerializeStringList(productoDto.ImagenesSecundarias),
                    VideoDemo = productoDto.VideoDemo,
                    Caracteristicas = SerializeStringList(productoDto.Caracteristicas),
                    Beneficios = SerializeStringList(productoDto.Beneficios),
                    Activo = true
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Producto creado exitosamente",
                    data = producto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear el producto",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActualizarProducto(int id, [FromBody] ProductoUpdateDto productoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                producto.Nombre = productoDto.Nombre;
                producto.Descripcion = productoDto.Descripcion;
                producto.DescripcionDetallada = productoDto.DescripcionDetallada;
                producto.PrecioBase = productoDto.PrecioBase;
                producto.PorcentajeGanancia = productoDto.PorcentajeGanancia;
                producto.PrecioVenta = productoDto.PrecioBase * (1 + productoDto.PorcentajeGanancia / 100);
                producto.ImagenPrincipal = productoDto.ImagenPrincipal;
                producto.ImagenesSecundarias = SerializeStringList(productoDto.ImagenesSecundarias);
                producto.VideoDemo = productoDto.VideoDemo;
                producto.Caracteristicas = SerializeStringList(productoDto.Caracteristicas);
                producto.Beneficios = SerializeStringList(productoDto.Beneficios);
                producto.Activo = productoDto.Activo;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Producto actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al actualizar el producto",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                // Soft delete - solo marcamos como inactivo
                producto.Activo = false;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Producto eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al eliminar el producto",
                    error = ex.Message
                });
            }
        }

        [HttpPost("{id}/comentarios")]
        [Authorize]
        public async Task<IActionResult> AgregarComentario(int id, [FromBody] ComentarioCreateDto comentarioDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var comentario = new Comentario
                {
                    UsuarioId = userId,
                    ProductoId = id,
                    Calificacion = comentarioDto.Calificacion,
                    Contenido = comentarioDto.Contenido,
                    Aprobado = false, // Requiere aprobación del admin
                    Activo = true
                };

                _context.Comentarios.Add(comentario);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Comentario enviado exitosamente. Será revisado antes de publicarse."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al agregar el comentario",
                    error = ex.Message
                });
            }
        }

        // Métodos auxiliares para serialización
        private List<string> DeserializeStringList(string? jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(jsonString) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private string? SerializeStringList(List<string>? list)
        {
            if (list == null || !list.Any())
                return null;

            try
            {
                return JsonSerializer.Serialize(list);
            }
            catch
            {
                return null;
            }
        }
    }
}