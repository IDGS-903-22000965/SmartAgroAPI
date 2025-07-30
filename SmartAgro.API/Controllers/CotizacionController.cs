using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAgro.API.Services;
using SmartAgro.Models.DTOs;
using System.Security.Claims;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CotizacionController : ControllerBase
    {
        private readonly ICotizacionService _cotizacionService;
        private readonly ILogger<CotizacionController> _logger;

        public CotizacionController(ICotizacionService cotizacionService, ILogger<CotizacionController> logger)
        {
            _cotizacionService = cotizacionService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CrearCotizacion([FromBody] CotizacionRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("🔄 Creando cotización para cliente: {Email}", request.EmailCliente);

                var cotizacion = await _cotizacionService.CrearCotizacionAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "Cotización creada exitosamente",
                    data = cotizacion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear cotización para: {Email}", request.EmailCliente);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear la cotización",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObtenerCotizaciones()
        {
            try
            {
                var cotizaciones = await _cotizacionService.ObtenerCotizacionesAsync();
                return Ok(new
                {
                    success = true,
                    data = cotizaciones
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cotizaciones");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener las cotizaciones",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerCotizacionPorId(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.ObtenerCotizacionPorIdAsync(id);
                if (cotizacion == null)
                    return NotFound(new { success = false, message = "Cotización no encontrada" });

                return Ok(new
                {
                    success = true,
                    data = cotizacion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cotización {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener la cotización",
                    error = ex.Message
                });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        [Authorize]
        public async Task<IActionResult> ObtenerCotizacionesPorUsuario(string usuarioId)
        {
            // Verificar que el usuario solo pueda ver sus propias cotizaciones
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != usuarioId)
                return Forbid();

            try
            {
                var cotizaciones = await _cotizacionService.ObtenerCotizacionesPorUsuarioAsync(usuarioId);
                return Ok(new
                {
                    success = true,
                    data = cotizaciones
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cotizaciones del usuario {UserId}", usuarioId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener las cotizaciones",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] EstadoCotizacionDto dto)
        {
            try
            {
                var result = await _cotizacionService.ActualizarEstadoCotizacionAsync(id, dto.Estado);
                if (!result)
                    return NotFound(new { success = false, message = "Cotización no encontrada" });

                return Ok(new
                {
                    success = true,
                    message = "Estado actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar estado de cotización {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al actualizar el estado",
                    error = ex.Message
                });
            }
        }

        [HttpPost("calcular-costo")]
        public async Task<IActionResult> CalcularCosto([FromBody] CotizacionRequestDto request)
        {
            // ✅ VALIDACIÓN MEJORADA
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("❌ Modelo inválido para calcular costo: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(new
                {
                    success = false,
                    message = "Datos inválidos",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                _logger.LogInformation("🔄 Calculando costo para área: {Area}m², cultivo: {Cultivo}",
                    request.AreaCultivo, request.TipoCultivo);

                var costo = await _cotizacionService.CalcularCostoCotizacionAsync(request);
                var costoConIva = costo * 1.16m;

                _logger.LogInformation("✅ Costo calculado: ${Costo} (con IVA: ${CostoIVA})", costo, costoConIva);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        costo = Math.Round(costo, 2),
                        costoConIva = Math.Round(costoConIva, 2),
                        detalles = new
                        {
                            areaCultivo = request.AreaCultivo,
                            tipoCultivo = request.TipoCultivo,
                            tipoSuelo = request.TipoSuelo,
                            fuenteAgua = request.FuenteAguaDisponible,
                            energia = request.EnergiaElectricaDisponible
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al calcular costo para área: {Area}m²", request.AreaCultivo);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al calcular el costo",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }
    }
}