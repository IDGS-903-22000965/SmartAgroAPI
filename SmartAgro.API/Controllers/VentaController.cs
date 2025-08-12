// VentaController.cs - VERSIÓN COMPLETA CORREGIDA
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models;
using SmartAgro.Models.DTOs;
using SmartAgro.Models.DTOs.Ventas;
using SmartAgro.Models.Entities;
using System.Security.Claims;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentaController : ControllerBase
    {
        private readonly SmartAgroDbContext _context;
        private readonly ILogger<VentaController> _logger;

        public VentaController(
            SmartAgroDbContext context,
            ILogger<VentaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============= MÉTODOS CRUD BÁSICOS =============

        /// <summary>
        /// Obtiene ventas con filtros y paginación
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Empleado")]
        public async Task<ActionResult> GetVentas(
            [FromQuery] string? searchTerm,
            [FromQuery] string? estado,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] string? metodoPago,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Ventas
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Include(v => v.Cotizacion)
                    .Include(v => v.Usuario)
                    .AsQueryable();

                // Filtros
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(v =>
                        v.NumeroVenta.Contains(searchTerm) ||
                        v.NombreCliente.Contains(searchTerm) ||
                        (v.EmailCliente != null && v.EmailCliente.Contains(searchTerm)));
                }

                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(v => v.EstadoVenta == estado);
                }

                if (fechaInicio.HasValue)
                {
                    query = query.Where(v => v.FechaVenta >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(v => v.FechaVenta <= fechaFin.Value);
                }

                if (!string.IsNullOrEmpty(metodoPago))
                {
                    query = query.Where(v => v.MetodoPago == metodoPago);
                }

                // Paginación
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var ventas = await query
                    .OrderByDescending(v => v.FechaVenta)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(v => new VentaDto
                    {
                        Id = v.Id,
                        NumeroVenta = v.NumeroVenta,
                        NombreCliente = v.NombreCliente,
                        EmailCliente = v.EmailCliente,
                        Total = v.Total,
                        FechaVenta = v.FechaVenta,
                        EstadoVenta = v.EstadoVenta,
                        MetodoPago = v.MetodoPago,
                        CantidadItems = v.Detalles.Count(),
                        NumeroCotizacion = v.Cotizacion != null ? v.Cotizacion.NumeroCotizacion : null
                    })
                    .ToListAsync();

                return Ok(new
                {
                    ventas = ventas,
                    totalCount = totalCount,
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    totalPages = totalPages,
                    hasNextPage = pageNumber < totalPages,
                    hasPreviousPage = pageNumber > 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas");
                return StatusCode(500, new { message = "Error al obtener las ventas" });
            }
        }

        /// <summary>
        /// Obtiene una venta específica por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Empleado")]
        public async Task<ActionResult> GetVentaById(int id)
        {
            try
            {
                var venta = await _context.Ventas
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Include(v => v.Cotizacion)
                    .Include(v => v.Usuario)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (venta == null)
                    return NotFound(new { message = "Venta no encontrada" });

                var ventaDetail = new VentaDetalleDto
                {
                    Id = venta.Id,
                    NumeroVenta = venta.NumeroVenta,
                    UsuarioId = venta.UsuarioId,
                    NombreUsuario = venta.Usuario?.Nombre ?? "Usuario no encontrado",
                    CotizacionId = venta.CotizacionId,
                    NumeroCotizacion = venta.Cotizacion?.NumeroCotizacion,
                    NombreCliente = venta.NombreCliente,
                    EmailCliente = venta.EmailCliente,
                    TelefonoCliente = venta.TelefonoCliente,
                    DireccionEntrega = venta.DireccionEntrega,
                    Subtotal = venta.Subtotal,
                    Impuestos = venta.Impuestos,
                    Total = venta.Total,
                    FechaVenta = venta.FechaVenta,
                    EstadoVenta = venta.EstadoVenta,
                    MetodoPago = venta.MetodoPago,
                    Observaciones = venta.Observaciones,
                    Detalles = venta.Detalles.Select(d => new DetalleVentaDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        NombreProducto = d.Producto.Nombre,
                        DescripcionProducto = d.Producto.Descripcion,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal,
                        ImagenProducto = d.Producto.ImagenPrincipal
                    }).ToList()
                };

                return Ok(new { data = ventaDetail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener venta {VentaId}", id);
                return StatusCode(500, new { message = "Error al obtener la venta" });
            }
        }

        /// <summary>
        /// Obtiene las ventas del usuario actual (para clientes)
        /// </summary>
        [HttpGet("mis-ventas")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult> GetMisVentas()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var ventas = await _context.Ventas
                    .Where(v => v.UsuarioId == currentUserId)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Include(v => v.Cotizacion)
                    .OrderByDescending(v => v.FechaVenta)
                    .Select(v => new VentaDto
                    {
                        Id = v.Id,
                        NumeroVenta = v.NumeroVenta,
                        NombreCliente = v.NombreCliente,
                        EmailCliente = v.EmailCliente,
                        Total = v.Total,
                        FechaVenta = v.FechaVenta,
                        EstadoVenta = v.EstadoVenta,
                        MetodoPago = v.MetodoPago,
                        CantidadItems = v.Detalles.Count(),
                        NumeroCotizacion = v.Cotizacion != null ? v.Cotizacion.NumeroCotizacion : null
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = ventas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas del usuario");
                return StatusCode(500, new { success = false, message = "Error al obtener las ventas" });
            }
        }

        /// <summary>
        /// Crea una nueva venta
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Empleado")]
        public async Task<ActionResult> CrearVenta([FromBody] CrearVentaDto request)
        {
            try
            {
                var numeroVenta = await GenerarNumeroVenta();

                var venta = new Venta
                {
                    NumeroVenta = numeroVenta,
                    UsuarioId = request.UsuarioId ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    NombreCliente = request.NombreCliente,
                    EmailCliente = request.EmailCliente,
                    TelefonoCliente = request.TelefonoCliente,
                    DireccionEntrega = request.DireccionEntrega,
                    MetodoPago = request.MetodoPago,
                    Observaciones = request.Observaciones,
                    Subtotal = request.Detalles.Sum(d => d.Subtotal),
                    Impuestos = request.Detalles.Sum(d => d.Subtotal) * 0.16m, // 16% IVA
                    Total = request.Detalles.Sum(d => d.Subtotal) * 1.16m,
                    FechaVenta = DateTime.Now,
                    EstadoVenta = "Pendiente",
                    Detalles = request.Detalles.Select(d => new DetalleVenta
                    {
                        ProductoId = d.ProductoId,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    }).ToList()
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = new { ventaId = venta.Id, numeroVenta = venta.NumeroVenta } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear venta");
                return StatusCode(500, new { success = false, message = "Error al crear la venta" });
            }
        }

        /// <summary>
        /// 🔥 MÉTODO CORREGIDO - Crea una venta desde una cotización
        /// </summary>
        [HttpPost("desde-cotizacion/{cotizacionId}")]
        [Authorize(Roles = "Admin,Empleado")]
        public async Task<ActionResult> CrearVentaDesdeCotizacion(
            int cotizacionId,
            [FromBody] CreateVentaFromCotizacionDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando conversión de cotización {CotizacionId} a venta", cotizacionId);

                // Obtener la cotización con sus detalles
                var cotizacion = await _context.Cotizaciones
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Include(c => c.Usuario) // ✅ IMPORTANTE: Incluir el usuario
                    .FirstOrDefaultAsync(c => c.Id == cotizacionId);

                if (cotizacion == null)
                {
                    _logger.LogWarning("Cotización {CotizacionId} no encontrada", cotizacionId);
                    return NotFound(new
                    {
                        success = false,
                        message = "Cotización no encontrada"
                    });
                }

                // ✅ VALIDAR que la cotización tenga un usuario válido
                if (string.IsNullOrEmpty(cotizacion.UsuarioId))
                {
                    _logger.LogError("Cotización {CotizacionId} no tiene un usuario válido asociado", cotizacionId);
                    return BadRequest(new
                    {
                        success = false,
                        message = "La cotización no tiene un usuario válido asociado"
                    });
                }

                // Verificar que la cotización no haya sido ya convertida
                var ventaExistente = await _context.Ventas
                    .FirstOrDefaultAsync(v => v.CotizacionId == cotizacionId);

                if (ventaExistente != null)
                {
                    _logger.LogWarning("Cotización {CotizacionId} ya fue convertida a venta {VentaId}",
                        cotizacionId, ventaExistente.Id);
                    return BadRequest(new
                    {
                        success = false,
                        message = "Esta cotización ya fue convertida a venta",
                        ventaExistente = new
                        {
                            id = ventaExistente.Id,
                            numeroVenta = ventaExistente.NumeroVenta
                        }
                    });
                }

                // Generar número de venta único
                var numeroVenta = await GenerarNumeroVenta();

                // ✅ CREAR LA VENTA CON EL USUARIO ORIGINAL DE LA COTIZACIÓN
                var venta = new Venta
                {
                    NumeroVenta = numeroVenta,
                    CotizacionId = cotizacion.Id,

                    // 🔥 ESTA ES LA LÍNEA CLAVE - USAR EL USUARIO DE LA COTIZACIÓN
                    UsuarioId = cotizacion.UsuarioId,

                    // Datos del cliente desde la cotización
                    NombreCliente = cotizacion.NombreCliente,
                    EmailCliente = cotizacion.EmailCliente,
                    TelefonoCliente = cotizacion.TelefonoCliente,

                    // Datos de la venta
                    DireccionEntrega = request.DireccionEntrega ?? cotizacion.DireccionInstalacion,
                    MetodoPago = request.MetodoPago,
                    Observaciones = request.Observaciones,

                    // Montos
                    Subtotal = cotizacion.Subtotal,
                    Impuestos = cotizacion.Impuestos,
                    Total = cotizacion.Total,

                    // Fechas y estado
                    FechaVenta = DateTime.Now,
                    EstadoVenta = "Pendiente",

                    // Detalles de la venta
                    Detalles = cotizacion.Detalles.Select(d => new DetalleVenta
                    {
                        ProductoId = d.ProductoId,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    }).ToList()
                };

                // Agregar la venta al contexto
                _context.Ventas.Add(venta);

                // ✅ ACTUALIZAR SOLO EL ESTADO (sin FechaModificacion)
                cotizacion.Estado = "Convertida";

                // Guardar cambios
                await _context.SaveChangesAsync();

                // ✅ LOG CORREGIDO
                _logger.LogInformation(
                    "✅ Venta creada exitosamente desde cotización. " +
                    "VentaId: {VentaId}, NumeroVenta: {NumeroVenta}, CotizacionId: {CotizacionId}, " +
                    "ClienteOriginal: {ClienteId}, ClienteNombre: {ClienteNombre}, AdminProceso: {AdminId}",
                    venta.Id, venta.NumeroVenta, cotizacionId,
                    cotizacion.UsuarioId, cotizacion.NombreCliente,
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        ventaId = venta.Id,
                        numeroVenta = venta.NumeroVenta,
                        clienteOriginal = cotizacion.UsuarioId,
                        clienteNombre = cotizacion.NombreCliente,
                        adminProceso = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        total = venta.Total
                    },
                    message = "Venta creada exitosamente desde cotización"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear venta desde cotización {CotizacionId}", cotizacionId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor al crear la venta"
                });
            }
        }

        /// <summary>
        /// Actualiza el estado de una venta
        /// </summary>
        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Admin,Empleado")]
        public async Task<ActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoVentaDto request)
        {
            try
            {
                var venta = await _context.Ventas.FindAsync(id);
                if (venta == null)
                    return NotFound(new { message = "Venta no encontrada" });

                venta.EstadoVenta = request.EstadoVenta;
                if (!string.IsNullOrEmpty(request.Observaciones))
                {
                    venta.Observaciones = request.Observaciones;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Estado de venta {VentaId} actualizado a {Estado}", id, request.EstadoVenta);

                return Ok(new { success = true, message = "Estado actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de venta {VentaId}", id);
                return StatusCode(500, new { message = "Error al actualizar el estado" });
            }
        }

        // ============= ESTADÍSTICAS =============

        /// <summary>
        /// Obtiene estadísticas generales de ventas
        /// </summary>
        [HttpGet("estadisticas")]
        [Authorize(Roles = "Admin,Empleado")]
        public async Task<ActionResult> GetEstadisticas()
        {
            try
            {
                var totalVentas = await _context.Ventas.CountAsync();
                var montoTotalVentas = await _context.Ventas.SumAsync(v => v.Total);

                var hoy = DateTime.Today;
                var ventasHoy = await _context.Ventas.CountAsync(v => v.FechaVenta.Date == hoy);
                var montoVentasHoy = await _context.Ventas
                    .Where(v => v.FechaVenta.Date == hoy)
                    .SumAsync(v => v.Total);

                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var ventasEsteMes = await _context.Ventas.CountAsync(v => v.FechaVenta >= inicioMes);
                var montoVentasEsteMes = await _context.Ventas
                    .Where(v => v.FechaVenta >= inicioMes)
                    .SumAsync(v => v.Total);

                var ventasPendientes = await _context.Ventas.CountAsync(v => v.EstadoVenta == "Pendiente");
                var ventasCompletadas = await _context.Ventas.CountAsync(v => v.EstadoVenta == "Entregado");

                var estadisticas = new EstadisticasVentasDto
                {
                    TotalVentas = totalVentas,
                    MontoTotalVentas = montoTotalVentas,
                    VentasHoy = ventasHoy,
                    MontoVentasHoy = montoVentasHoy,
                    VentasEsteMes = ventasEsteMes,
                    MontoVentasEsteMes = montoVentasEsteMes,
                    VentasPendientes = ventasPendientes,
                    VentasCompletadas = ventasCompletadas,
                    PromedioVentaDiaria = totalVentas > 0 ? montoTotalVentas / totalVentas : 0
                };

                return Ok(new { data = estadisticas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de ventas");
                return StatusCode(500, new { message = "Error al obtener estadísticas" });
            }
        }
        [HttpPost("{productoId}/documentos")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> AgregarDocumento(int productoId, [FromBody] AgregarDocumentoDto dto)
{
    var doc = new ProductoDocumento
    {
        ProductoId = productoId,
        Titulo = dto.Titulo,
        Tipo = dto.Tipo,
        Url = dto.Url
    };

    _context.ProductoDocumentos.Add(doc);
    await _context.SaveChangesAsync();

    return Ok(new { success = true, message = "Documento agregado" });
}

        /// <summary>
        /// Genera un número de venta único
        /// </summary>
        private async Task<string> GenerarNumeroVenta()
        {
            var ultimaVenta = await _context.Ventas
                .OrderByDescending(v => v.Id)
                .FirstOrDefaultAsync();

            var ultimoNumero = 0;
            if (ultimaVenta != null && ultimaVenta.NumeroVenta.StartsWith("VNT-"))
            {
                var numeroParte = ultimaVenta.NumeroVenta.Substring(4);
                int.TryParse(numeroParte, out ultimoNumero);
            }

            return $"VNT-{(ultimoNumero + 1):D6}";
        }

        /// <summary>
        /// Genera número de venta (endpoint público para el frontend)
        /// </summary>
        [HttpGet("generar-numero")]
        [Authorize(Roles = "Admin,Empleado")]
        public async Task<ActionResult> GenerarNumeroVentaEndpoint()
        {
            try
            {
                var numeroVenta = await GenerarNumeroVenta();
                return Ok(new { success = true, data = numeroVenta });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar número de venta");
                return StatusCode(500, new { success = false, message = "Error al generar número de venta" });
            }
        }
    }

    // ============= DTOs =============

    public class CreateVentaFromCotizacionDto
    {
        public string MetodoPago { get; set; } = string.Empty;
        public string? DireccionEntrega { get; set; }
        public string? Observaciones { get; set; }
    }

    public class CrearVentaDto
    {
        public string? UsuarioId { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string? EmailCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public string? DireccionEntrega { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public List<CrearDetalleVentaDto> Detalles { get; set; } = new();
    }

    public class CrearDetalleVentaDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class ActualizarEstadoVentaDto
    {
        public string EstadoVenta { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }

    public class VentaDto
    {
        public int Id { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string? EmailCliente { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaVenta { get; set; }
        public string EstadoVenta { get; set; } = string.Empty;
        public string? MetodoPago { get; set; }
        public int CantidadItems { get; set; }
        public string? NumeroCotizacion { get; set; }
    }

    public class VentaDetalleDto
    {
        public int Id { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public int? CotizacionId { get; set; }
        public string? NumeroCotizacion { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string? EmailCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public string? DireccionEntrega { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaVenta { get; set; }
        public string EstadoVenta { get; set; } = string.Empty;
        public string? MetodoPago { get; set; }
        public string? Observaciones { get; set; }
        public List<DetalleVentaDto> Detalles { get; set; } = new();
    }

    public class DetalleVentaDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? DescripcionProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string? ImagenProducto { get; set; }
    }

    public class EstadisticasVentasDto
    {
        public int TotalVentas { get; set; }
        public decimal MontoTotalVentas { get; set; }
        public int VentasHoy { get; set; }
        public decimal MontoVentasHoy { get; set; }
        public int VentasEsteMes { get; set; }
        public decimal MontoVentasEsteMes { get; set; }
        public int VentasPendientes { get; set; }
        public int VentasCompletadas { get; set; }
        public decimal PromedioVentaDiaria { get; set; }
        public decimal CrecimientoMesAnterior { get; set; }
        public Dictionary<string, int> VentasPorEstado { get; set; } = new();
        public Dictionary<string, int> VentasPorMetodoPago { get; set; } = new();
    }
}