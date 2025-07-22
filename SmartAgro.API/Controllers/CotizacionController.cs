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

        public CotizacionController(ICotizacionService cotizacionService)
        {
            _cotizacionService = cotizacionService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearCotizacion([FromBody] CotizacionRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
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
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] string estado)
        {
            try
            {
                var result = await _cotizacionService.ActualizarEstadoCotizacionAsync(id, estado);
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
            try
            {
                var costo = await _cotizacionService.CalcularCostoCotizacionAsync(request);
                return Ok(new
                {
                    success = true,
                    costo = costo,
                    costoConIva = costo * 1.16m
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al calcular el costo",
                    error = ex.Message
                });
            }
        }
    }
}