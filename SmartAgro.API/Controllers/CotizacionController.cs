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

        public CotizacionController(
            ICotizacionService cotizacionService,
            ILogger<CotizacionController> logger)
        {
            _cotizacionService = cotizacionService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> CrearCotizacion([FromBody] CotizacionRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("❌ Modelo inválido para crear cotización: {Errors}",
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
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogWarning("❌ Usuario no autenticado intentando crear cotización");
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Usuario no autenticado"
                    });
                }

                _logger.LogInformation("🔄 Creando cotización para usuario: {UserId}, Cliente: {Cliente}",
                    currentUserId, request.NombreCliente);

                // ✅ LLAMAR AL SERVICIO
                var cotizacion = await _cotizacionService.CrearCotizacionAsync(request, currentUserId);

                _logger.LogInformation("✅ Cotización creada exitosamente: {Numero}, Usuario: {UserId}",
                    cotizacion.NumeroCotizacion, currentUserId);

                // 🔥 SOLUCIÓN: Devolver un DTO simple sin referencias circulares
                var response = new
                {
                    success = true,
                    data = new
                    {
                        id = cotizacion.Id,
                        numeroCotizacion = cotizacion.NumeroCotizacion,
                        nombreCliente = cotizacion.NombreCliente,
                        emailCliente = cotizacion.EmailCliente,
                        total = cotizacion.Total,
                        estado = cotizacion.Estado,
                        fechaCotizacion = cotizacion.FechaCotizacion,
                        fechaVencimiento = cotizacion.FechaVencimiento
                    },
                    message = "Cotización creada exitosamente"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear cotización para usuario: {UserId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor al crear la cotización",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Calcular costo de cotización
        /// </summary>
        [HttpPost("calcular-costo")]
        public async Task<ActionResult> CalcularCosto([FromBody] CotizacionRequestDto request)
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

        /// <summary>
        /// Obtener todas las cotizaciones (Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Empleado")]
        public async Task<ActionResult> ObtenerCotizaciones()
        {
            try
            {
                var cotizaciones = await _cotizacionService.ObtenerCotizacionesAsync();
                return Ok(new { success = true, data = cotizaciones });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cotizaciones");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener cotizaciones",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener cotización por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> ObtenerCotizacionPorId(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.ObtenerCotizacionPorIdAsync(id);

                if (cotizacion == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cotización no encontrada"
                    });
                }

                return Ok(new { success = true, data = cotizacion });
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

        /// <summary>
        /// Obtener cotizaciones del usuario actual
        /// </summary>
        [HttpGet("usuario/{usuarioId}")]
        [Authorize]
        public async Task<ActionResult> ObtenerCotizacionesPorUsuario(string usuarioId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Verificar que el usuario solo pueda ver sus propias cotizaciones
                // (a menos que sea admin)
                if (currentUserId != usuarioId && !User.IsInRole("Admin") && !User.IsInRole("Empleado"))
                {
                    return Forbid();
                }

                var cotizaciones = await _cotizacionService.ObtenerCotizacionesPorUsuarioAsync(usuarioId);
                return Ok(new { success = true, data = cotizaciones });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cotizaciones del usuario {UserId}", usuarioId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener cotizaciones del usuario",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Actualizar estado de cotización
        /// </summary>
        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Admin,Empleado")]
        public async Task<ActionResult> ActualizarEstado(int id, [FromBody] EstadoCotizacionDto request)
        {
            try
            {
                var actualizado = await _cotizacionService.ActualizarEstadoCotizacionAsync(id, request.Estado);

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cotización no encontrada"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Estado actualizado correctamente"
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
    }

    // DTOs necesarios
    public class EstadoCotizacionDto
    {
        public string Estado { get; set; } = string.Empty;
    }
}