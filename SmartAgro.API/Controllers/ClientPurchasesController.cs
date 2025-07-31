// SmartAgro.API/Controllers/ClientPurchasesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.DTOs.Ventas;
using System.Security.Claims;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/client/purchases")]
    [Authorize(Roles = "Cliente")]
    public class ClientPurchasesController : ControllerBase
    {
        private readonly SmartAgroDbContext _context;
        private readonly ILogger<ClientPurchasesController> _logger;

        public ClientPurchasesController(
            SmartAgroDbContext context,
            ILogger<ClientPurchasesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las compras del cliente actual
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ClientPurchaseDto>>> GetMyPurchases()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var purchases = await _context.Ventas
                    .Where(v => v.UsuarioId == currentUserId)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Include(v => v.Cotizacion)
                    .OrderByDescending(v => v.FechaVenta)
                    .Select(v => new ClientPurchaseDto
                    {
                        Id = v.Id,
                        NumeroVenta = v.NumeroVenta,
                        FechaVenta = v.FechaVenta,
                        EstadoVenta = v.EstadoVenta,
                        MetodoPago = v.MetodoPago,
                        Subtotal = v.Subtotal,
                        Impuestos = v.Impuestos,
                        Total = v.Total,
                        DireccionEntrega = v.DireccionEntrega,
                        Observaciones = v.Observaciones,
                        NumeroCotizacion = v.Cotizacion != null ? v.Cotizacion.NumeroCotizacion : null,
                        CantidadProductos = v.Detalles.Count(),
                        Productos = v.Detalles.Select(d => new ClientPurchaseProductDto
                        {
                            ProductoId = d.ProductoId,
                            NombreProducto = d.Producto.Nombre,
                            DescripcionProducto = d.Producto.Descripcion,
                            ImagenProducto = d.Producto.ImagenPrincipal,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = purchases });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener compras del cliente");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener compras"
                });
            }
        }

        /// <summary>
        /// Obtiene el detalle de una compra específica
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientPurchaseDetailDto>> GetPurchaseDetail(int id)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var purchase = await _context.Ventas
                    .Where(v => v.Id == id && v.UsuarioId == currentUserId)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Include(v => v.Cotizacion)
                    .Include(v => v.Usuario)
                    .FirstOrDefaultAsync();

                if (purchase == null)
                    return NotFound(new
                    {
                        success = false,
                        message = "Compra no encontrada"
                    });

                var purchaseDetail = new ClientPurchaseDetailDto
                {
                    Id = purchase.Id,
                    NumeroVenta = purchase.NumeroVenta,
                    FechaVenta = purchase.FechaVenta,
                    EstadoVenta = purchase.EstadoVenta,
                    MetodoPago = purchase.MetodoPago,
                    Subtotal = purchase.Subtotal,
                    Impuestos = purchase.Impuestos,
                    Total = purchase.Total,
                    DireccionEntrega = purchase.DireccionEntrega,
                    Observaciones = purchase.Observaciones,
                    NumeroCotizacion = purchase.Cotizacion?.NumeroCotizacion,

                    // Datos del cliente
                    NombreCliente = purchase.NombreCliente,
                    EmailCliente = purchase.EmailCliente,
                    TelefonoCliente = purchase.TelefonoCliente,

                    // Productos - deserializar después de la consulta
                    Productos = purchase.Detalles.Select(d => new ClientPurchaseProductDetailDto
                    {
                        ProductoId = d.ProductoId,
                        NombreProducto = d.Producto.Nombre,
                        DescripcionProducto = d.Producto.Descripcion,
                        DescripcionDetallada = d.Producto.DescripcionDetallada,
                        ImagenPrincipal = d.Producto.ImagenPrincipal,
                        ImagenesSecundarias = DeserializeStringList(d.Producto.ImagenesSecundarias),
                        VideoDemo = d.Producto.VideoDemo,
                        Caracteristicas = DeserializeStringList(d.Producto.Caracteristicas),
                        Beneficios = DeserializeStringList(d.Producto.Beneficios),
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal,

                        // Verificar si el cliente ya comentó este producto
                        YaComento = _context.Comentarios.Any(c =>
                            c.UsuarioId == currentUserId &&
                            c.ProductoId == d.ProductoId)
                    }).ToList()
                };

                return Ok(new { success = true, data = purchaseDetail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de compra {PurchaseId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener detalle de compra"
                });
            }
        }

        /// <summary>
        /// Obtiene todos los productos únicos que ha comprado el cliente
        /// </summary>
        [HttpGet("products")]
        public async Task<ActionResult<List<ClientOwnedProductDto>>> GetOwnedProducts()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                // Primero obtenemos los datos básicos sin deserializar
                var ownedProductsQuery = await _context.DetallesVenta
                    .Where(d => d.Venta.UsuarioId == currentUserId)
                    .Include(d => d.Producto)
                    .Include(d => d.Venta)
                    .GroupBy(d => d.ProductoId)
                    .Select(g => new
                    {
                        ProductoId = g.Key,
                        Producto = g.First().Producto,
                        TotalComprado = g.Sum(d => d.Cantidad),
                        TotalGastado = g.Sum(d => d.Subtotal),
                        NumeroCompras = g.Select(d => d.VentaId).Distinct().Count(),
                        PrimeraCompra = g.Min(d => d.Venta.FechaVenta),
                        UltimaCompra = g.Max(d => d.Venta.FechaVenta)
                    })
                    .OrderByDescending(p => p.UltimaCompra)
                    .ToListAsync();

                // Ahora procesamos los resultados y deserializamos
                var ownedProducts = new List<ClientOwnedProductDto>();

                foreach (var item in ownedProductsQuery)
                {
                    var yaComento = await _context.Comentarios
                        .AnyAsync(c => c.UsuarioId == currentUserId && c.ProductoId == item.ProductoId);

                    var calificacionPromedio = await _context.Comentarios
                        .Where(c => c.ProductoId == item.ProductoId && c.Aprobado)
                        .AverageAsync(c => (double?)c.Calificacion) ?? 0;

                    var totalComentarios = await _context.Comentarios
                        .CountAsync(c => c.ProductoId == item.ProductoId && c.Aprobado);

                    ownedProducts.Add(new ClientOwnedProductDto
                    {
                        ProductoId = item.ProductoId,
                        NombreProducto = item.Producto.Nombre,
                        DescripcionProducto = item.Producto.Descripcion,
                        DescripcionDetallada = item.Producto.DescripcionDetallada,
                        ImagenPrincipal = item.Producto.ImagenPrincipal,
                        ImagenesSecundarias = DeserializeStringList(item.Producto.ImagenesSecundarias),
                        VideoDemo = item.Producto.VideoDemo,
                        Caracteristicas = DeserializeStringList(item.Producto.Caracteristicas),
                        Beneficios = DeserializeStringList(item.Producto.Beneficios),
                        TotalComprado = item.TotalComprado,
                        TotalGastado = item.TotalGastado,
                        NumeroCompras = item.NumeroCompras,
                        PrimeraCompra = item.PrimeraCompra,
                        UltimaCompra = item.UltimaCompra,
                        YaComento = yaComento,
                        CalificacionPromedio = calificacionPromedio,
                        TotalComentarios = totalComentarios
                    });
                }

                return Ok(new { success = true, data = ownedProducts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos del cliente");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener productos"
                });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de compras del cliente
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ClientPurchaseStatsDto>> GetPurchaseStats()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var purchases = await _context.Ventas
                    .Where(v => v.UsuarioId == currentUserId)
                    .ToListAsync();

                var totalCompras = purchases.Count;
                var totalGastado = purchases.Sum(v => v.Total);
                var comprasEsteMes = purchases.Count(v => v.FechaVenta.Month == DateTime.Now.Month &&
                                                         v.FechaVenta.Year == DateTime.Now.Year);
                var gastoEsteMes = purchases
                    .Where(v => v.FechaVenta.Month == DateTime.Now.Month &&
                               v.FechaVenta.Year == DateTime.Now.Year)
                    .Sum(v => v.Total);

                var productosFavoritos = await _context.DetallesVenta
                    .Where(d => d.Venta.UsuarioId == currentUserId)
                    .GroupBy(d => d.ProductoId)
                    .OrderByDescending(g => g.Sum(d => d.Cantidad))
                    .Take(3)
                    .Select(g => g.First().Producto.Nombre)
                    .ToListAsync();

                var stats = new ClientPurchaseStatsDto
                {
                    TotalCompras = totalCompras,
                    TotalGastado = totalGastado,
                    ComprasEsteMes = comprasEsteMes,
                    GastoEsteMes = gastoEsteMes,
                    PromedioGasto = totalCompras > 0 ? totalGastado / totalCompras : 0,
                    PrimeraCompra = purchases.Any() ? purchases.Min(v => v.FechaVenta) : null,
                    UltimaCompra = purchases.Any() ? purchases.Max(v => v.FechaVenta) : null,
                    ProductosFavoritos = productosFavoritos
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de compras");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener estadísticas"
                });
            }
        }

        // MÉTODO AHORA ESTÁTICO - Esta es la clave para solucionar el error
        private static List<string> DeserializeStringList(string? jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonString) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}