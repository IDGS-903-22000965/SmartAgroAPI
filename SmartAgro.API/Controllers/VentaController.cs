using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAgro.API.Services;
using SmartAgro.Models.DTOs.Ventas;
using SmartAgro.Models.DTOs;
using System.Security.Claims;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentaController : ControllerBase
    {
        private readonly IVentaService _ventaService;
        private readonly ILogger<VentaController> _logger;

        public VentaController(IVentaService ventaService, ILogger<VentaController> logger)
        {
            _ventaService = ventaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las ventas con filtros y paginación
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginatedVentasDto>> GetVentas(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? estado = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] string? metodoPago = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _ventaService.ObtenerVentasPaginadasAsync(
                    pageNumber, pageSize, searchTerm, estado, fechaInicio, fechaFin, metodoPago);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas");
                return StatusCode(500, new { message = "Error al obtener ventas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una venta específica por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<VentaDetalleDto>> GetVenta(int id)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");

                var venta = await _ventaService.ObtenerVentaDetalleAsync(id);
                if (venta == null)
                    return NotFound(new { message = "Venta no encontrada" });

                // Solo admin o el usuario propietario pueden ver la venta
                if (!isAdmin && venta.UsuarioId != currentUserId)
                    return Forbid();

                return Ok(new { success = true, data = venta });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener venta {VentaId}", id);
                return StatusCode(500, new { message = "Error al obtener venta", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva venta
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateVenta([FromBody] CreateVentaDto createVentaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                createVentaDto.UsuarioId = userId;
                var result = await _ventaService.CrearVentaAsync(createVentaDto);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { message = result.Message, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear venta");
                return StatusCode(500, new { message = "Error al crear venta", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el estado de una venta
        /// </summary>
        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEstadoVenta(int id, [FromBody] ActualizarEstadoVentaDto estadoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _ventaService.ActualizarEstadoVentaAsync(id, estadoDto);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de venta {VentaId}", id);
                return StatusCode(500, new { message = "Error al actualizar estado", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene las ventas del usuario actual
        /// </summary>
        [HttpGet("mis-ventas")]
        public async Task<ActionResult<List<VentaListDto>>> GetMisVentas()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var ventas = await _ventaService.ObtenerVentasPorUsuarioAsync(userId);
                return Ok(new { success = true, data = ventas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas del usuario");
                return StatusCode(500, new { message = "Error al obtener ventas", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una venta a partir de una cotización aprobada
        /// </summary>
        [HttpPost("desde-cotizacion/{cotizacionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateVentaFromCotizacion(int cotizacionId, [FromBody] CreateVentaFromCotizacionDto ventaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _ventaService.CrearVentaDesdeCotizacionAsync(cotizacionId, ventaDto);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { message = result.Message, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear venta desde cotización {CotizacionId}", cotizacionId);
                return StatusCode(500, new { message = "Error al crear venta", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene estadísticas generales de ventas
        /// </summary>
        [HttpGet("estadisticas")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<VentaStatsDto>> GetEstadisticasVentas()
        {
            try
            {
                var stats = await _ventaService.ObtenerEstadisticasVentasAsync();
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de ventas");
                return StatusCode(500, new { message = "Error al obtener estadísticas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene reporte de ventas por período
        /// </summary>
        [HttpGet("reporte")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ReporteVentasDto>> GetReporteVentas(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin,
            [FromQuery] string? agrupacion = "mes") // dia, mes, año
        {
            try
            {
                if (fechaInicio > fechaFin)
                    return BadRequest(new { message = "La fecha de inicio no puede ser mayor a la fecha final" });

                var reporte = await _ventaService.GenerarReporteVentasAsync(fechaInicio, fechaFin, agrupacion);
                return Ok(new { success = true, data = reporte });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de ventas");
                return StatusCode(500, new { message = "Error al generar reporte", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene productos más vendidos
        /// </summary>
        [HttpGet("productos-mas-vendidos")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<ProductoVentaDto>>> GetProductosMasVendidos(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int top = 10)
        {
            try
            {
                var productos = await _ventaService.ObtenerProductosMasVendidosAsync(fechaInicio, fechaFin, top);
                return Ok(new { success = true, data = productos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                return StatusCode(500, new { message = "Error al obtener productos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene clientes más frecuentes
        /// </summary>
        [HttpGet("clientes-frecuentes")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<ClienteVentaDto>>> GetClientesFrecuentes(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int top = 10)
        {
            try
            {
                var clientes = await _ventaService.ObtenerClientesFrecuentesAsync(fechaInicio, fechaFin, top);
                return Ok(new { success = true, data = clientes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes frecuentes");
                return StatusCode(500, new { message = "Error al obtener clientes", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene ventas por método de pago
        /// </summary>
        [HttpGet("por-metodo-pago")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<VentaPorMetodoPagoDto>>> GetVentasPorMetodoPago(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var datos = await _ventaService.ObtenerVentasPorMetodoPagoAsync(fechaInicio, fechaFin);
                return Ok(new { success = true, data = datos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por método de pago");
                return StatusCode(500, new { message = "Error al obtener datos", error = ex.Message });
            }
        }

        /// <summary>
        /// Genera número de venta único
        /// </summary>
        [HttpGet("generar-numero")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<string>> GenerarNumeroVenta()
        {
            try
            {
                var numero = await _ventaService.GenerarNumeroVentaAsync();
                return Ok(new { numeroVenta = numero });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar número de venta");
                return StatusCode(500, new { message = "Error al generar número", error = ex.Message });
            }
        }
    }
}