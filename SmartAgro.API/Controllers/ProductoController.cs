using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.DTOs;
using SmartAgro.Models.Entities;
using SmartAgro.API.Services;
using System.Security.Claims;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductoController : ControllerBase
    {
        private readonly SmartAgroDbContext _context;
        private readonly IProductoService _productoService;

        public ProductoController(SmartAgroDbContext context, IProductoService productoService)
        {
            _context = context;
            _productoService = productoService;
        }

        // ===== MÉTODOS BÁSICOS DE PRODUCTOS =====

        [HttpGet]
        public async Task<IActionResult> ObtenerProductos()
        {
            try
            {
                var productosDb = await _context.Productos
                    .Where(p => p.Activo)
                    .Include(p => p.Comentarios.Where(c => c.Aprobado && c.Activo))
                    .ThenInclude(c => c.Usuario)
                    .Include(p => p.ProductoMateriasPrimas)
                    .ThenInclude(pm => pm.MateriaPrima)
                    .ToListAsync();

                var productos = productosDb.Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    DescripcionDetallada = p.DescripcionDetallada,
                    PrecioBase = p.PrecioBase,
                    PorcentajeGanancia = p.PorcentajeGanancia,
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
                var productoDb = await _context.Productos
                    .Where(p => p.Id == id && p.Activo)
                    .Include(p => p.Comentarios.Where(c => c.Aprobado && c.Activo))
                    .ThenInclude(c => c.Usuario)
                    .Include(p => p.ProductoMateriasPrimas)
                    .ThenInclude(pm => pm.MateriaPrima)
                    .ThenInclude(mp => mp.Proveedor)
                    .FirstOrDefaultAsync();

                if (productoDb == null)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                var producto = new ProductoDetalleDto
                {
                    Id = productoDb.Id,
                    Nombre = productoDb.Nombre,
                    Descripcion = productoDb.Descripcion,
                    DescripcionDetallada = productoDb.DescripcionDetallada,
                    PrecioBase = productoDb.PrecioBase,
                    PorcentajeGanancia = productoDb.PorcentajeGanancia,
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

        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarProductos([FromQuery] string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                    return BadRequest(new { success = false, message = "El término de búsqueda es requerido" });

                var productos = await _productoService.BuscarProductosAsync(termino);

                var productosDto = productos.Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    DescripcionDetallada = p.DescripcionDetallada,
                    PrecioBase = p.PrecioBase,
                    PorcentajeGanancia = p.PorcentajeGanancia,
                    PrecioVenta = p.PrecioVenta,
                    ImagenPrincipal = p.ImagenPrincipal,
                    ImagenesSecundarias = DeserializeStringList(p.ImagenesSecundarias),
                    VideoDemo = p.VideoDemo,
                    Caracteristicas = DeserializeStringList(p.Caracteristicas),
                    Beneficios = DeserializeStringList(p.Beneficios),
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion
                }).ToList();

                return Ok(new { success = true, data = productosDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error en la búsqueda de productos",
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

                var nuevoProducto = await _productoService.CrearProductoAsync(producto);

                return Ok(new
                {
                    success = true,
                    message = "Producto creado exitosamente",
                    data = nuevoProducto
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

                await _productoService.ActualizarProductoAsync(producto);

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
                var resultado = await _productoService.EliminarProductoAsync(id);
                if (!resultado)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

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

        // ===== GESTIÓN DE RECETAS/EXPLOSIÓN DE MATERIALES =====

        /// <summary>
        /// Obtiene la receta (explosión de materiales) de un producto
        /// </summary>
        [HttpGet("{id}/receta")]
        public async Task<IActionResult> ObtenerRecetaProducto(int id)
        {
            try
            {
                var producto = await _productoService.ObtenerProductoPorIdAsync(id);
                if (producto == null)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                var receta = await _productoService.ObtenerRecetaProductoAsync(id);
                var costoTotal = await _productoService.CalcularPrecioCostoAsync(id);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        producto = new
                        {
                            id = producto.Id,
                            nombre = producto.Nombre,
                            precioBase = producto.PrecioBase,
                            precioVenta = producto.PrecioVenta,
                            porcentajeGanancia = producto.PorcentajeGanancia
                        },
                        receta = receta,
                        costoTotal = costoTotal,
                        rentabilidad = producto.PrecioVenta - costoTotal,
                        margenRentabilidad = costoTotal > 0 ? ((producto.PrecioVenta - costoTotal) / producto.PrecioVenta) * 100 : 0
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener la receta del producto",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Agrega una materia prima a la receta del producto
        /// </summary>
        [HttpPost("{id}/receta")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AgregarMateriaPrimaAReceta(int id, [FromBody] ProductoMateriaPrimaCreateDto materiaPrimaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var resultado = await _productoService.AgregarMateriaPrimaAsync(id, materiaPrimaDto);
                if (!resultado)
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se pudo agregar la materia prima. Verifique que el producto y la materia prima existan y que no esté duplicada."
                    });

                // Obtener datos actualizados para la respuesta
                var costoTotal = await _productoService.CalcularPrecioCostoAsync(id);
                var producto = await _productoService.ObtenerProductoPorIdAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Materia prima agregada a la receta exitosamente",
                    data = new
                    {
                        costoTotalActualizado = costoTotal,
                        precioVenta = producto?.PrecioVenta,
                        rentabilidad = (producto?.PrecioVenta ?? 0) - costoTotal
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al agregar materia prima a la receta",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Actualiza una materia prima en la receta del producto
        /// </summary>
        [HttpPut("{productoId}/receta/{materiaPrimaId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActualizarMateriaPrimaEnReceta(int productoId, int materiaPrimaId, [FromBody] ProductoMateriaPrimaCreateDto materiaPrimaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var resultado = await _productoService.ActualizarMateriaPrimaAsync(productoId, materiaPrimaId, materiaPrimaDto);
                if (!resultado)
                    return NotFound(new { success = false, message = "Materia prima no encontrada en la receta del producto" });

                // Obtener datos actualizados para la respuesta
                var costoTotal = await _productoService.CalcularPrecioCostoAsync(productoId);
                var producto = await _productoService.ObtenerProductoPorIdAsync(productoId);

                return Ok(new
                {
                    success = true,
                    message = "Materia prima actualizada en la receta exitosamente",
                    data = new
                    {
                        costoTotalActualizado = costoTotal,
                        precioVenta = producto?.PrecioVenta,
                        rentabilidad = (producto?.PrecioVenta ?? 0) - costoTotal
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al actualizar materia prima en la receta",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Elimina una materia prima de la receta del producto
        /// </summary>
        [HttpDelete("{productoId}/receta/{materiaPrimaId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EliminarMateriaPrimaDeReceta(int productoId, int materiaPrimaId)
        {
            try
            {
                var resultado = await _productoService.EliminarMateriaPrimaAsync(productoId, materiaPrimaId);
                if (!resultado)
                    return NotFound(new { success = false, message = "Materia prima no encontrada en la receta del producto" });

                // Obtener datos actualizados para la respuesta
                var costoTotal = await _productoService.CalcularPrecioCostoAsync(productoId);
                var producto = await _productoService.ObtenerProductoPorIdAsync(productoId);

                return Ok(new
                {
                    success = true,
                    message = "Materia prima eliminada de la receta exitosamente",
                    data = new
                    {
                        costoTotalActualizado = costoTotal,
                        precioVenta = producto?.PrecioVenta,
                        rentabilidad = (producto?.PrecioVenta ?? 0) - costoTotal
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al eliminar materia prima de la receta",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Recalcula el precio del producto basado en su receta
        /// </summary>
        [HttpPost("{id}/recalcular-precio")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecalcularPrecioProducto(int id)
        {
            try
            {
                var resultado = await _productoService.RecalcularPrecioProductoAsync(id);
                if (!resultado)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                var producto = await _productoService.ObtenerProductoPorIdAsync(id);
                var costoMateriales = await _productoService.CalcularPrecioCostoAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Precio recalculado exitosamente",
                    data = new
                    {
                        costoMateriales = costoMateriales,
                        precioBase = producto!.PrecioBase,
                        precioVenta = producto.PrecioVenta,
                        margenGanancia = producto.PorcentajeGanancia,
                        rentabilidad = producto.PrecioVenta - costoMateriales,
                        margenRentabilidad = costoMateriales > 0 ? ((producto.PrecioVenta - costoMateriales) / producto.PrecioVenta) * 100 : 0
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al recalcular el precio del producto",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Valida si hay suficiente stock para producir una cantidad específica del producto
        /// </summary>
        [HttpPost("{id}/validar-stock")]
        public async Task<IActionResult> ValidarStockParaProduccion(int id, [FromBody] ValidarStockDto validarStockDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tieneStock = await _productoService.ValidarStockParaProduccionAsync(id, validarStockDto.CantidadAProducir);
                var receta = await _productoService.ObtenerRecetaProductoAsync(id);

                var materialesFaltantes = new List<object>();
                var materialesDisponibles = new List<object>();

                foreach (var material in receta)
                {
                    var cantidadNecesaria = material.CantidadRequerida * validarStockDto.CantidadAProducir;

                    if (material.StockDisponible < cantidadNecesaria)
                    {
                        materialesFaltantes.Add(new
                        {
                            materiaPrima = material.NombreMateriaPrima,
                            unidadMedida = material.UnidadMedida,
                            cantidadNecesaria = cantidadNecesaria,
                            stockDisponible = material.StockDisponible,
                            faltante = cantidadNecesaria - material.StockDisponible,
                            proveedor = material.NombreProveedor
                        });
                    }
                    else
                    {
                        materialesDisponibles.Add(new
                        {
                            materiaPrima = material.NombreMateriaPrima,
                            unidadMedida = material.UnidadMedida,
                            cantidadNecesaria = cantidadNecesaria,
                            stockDisponible = material.StockDisponible
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    tieneStockSuficiente = tieneStock,
                    cantidadAProducir = validarStockDto.CantidadAProducir,
                    materialesDisponibles = materialesDisponibles,
                    materialesFaltantes = materialesFaltantes,
                    resumen = new
                    {
                        totalMateriales = receta.Count,
                        materialesOk = materialesDisponibles.Count,
                        materialesConFaltante = materialesFaltantes.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al validar stock para producción",
                    error = ex.Message
                });
            }
        }

        // ===== COMENTARIOS =====

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
                    Aprobado = false,
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

        // ===== MÉTODOS AUXILIARES =====

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

    // ===== DTOs AUXILIARES =====

    public class ValidarStockDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad a producir debe ser mayor a 0")]
        public int CantidadAProducir { get; set; }
    }
}