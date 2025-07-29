// SmartAgro.API/Controllers/ComprasProveedorController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAgro.API.Services;
using SmartAgro.Models.DTOs.ComprasProveedores;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ComprasProveedorController : ControllerBase
    {
        private readonly ICompraProveedorService _compraService;

        public ComprasProveedorController(ICompraProveedorService compraService)
        {
            _compraService = compraService;
        }

        /// <summary>
        /// Obtiene una lista paginada de compras con filtros opcionales
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedComprasDto>> GetCompras(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? proveedorId = null,
            [FromQuery] string? estado = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _compraService.ObtenerComprasAsync(pageNumber, pageSize, searchTerm, proveedorId, estado);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener compras: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene una compra específica por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CompraProveedorDetailsDto>> GetCompra(int id)
        {
            try
            {
                var compra = await _compraService.ObtenerCompraPorIdAsync(id);
                if (compra == null)
                {
                    return NotFound(new { message = "Compra no encontrada" });
                }

                return Ok(compra);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener compra: {ex.Message}" });
            }
        }

        /// <summary>
        /// Crea una nueva compra
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCompra([FromBody] CreateCompraProveedorDto createCompraDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _compraService.CrearCompraAsync(createCompraDto);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al crear compra: {ex.Message}" });
            }
        }

        /// <summary>
        /// Actualiza una compra existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompra(int id, [FromBody] UpdateCompraProveedorDto updateCompraDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _compraService.ActualizarCompraAsync(id, updateCompraDto);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al actualizar compra: {ex.Message}" });
            }
        }

        /// <summary>
        /// Elimina una compra
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompra(int id)
        {
            try
            {
                var result = await _compraService.EliminarCompraAsync(id);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al eliminar compra: {ex.Message}" });
            }
        }

        /// <summary>
        /// Cambia el estado de una compra
        /// </summary>
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto cambiarEstadoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _compraService.CambiarEstadoCompraAsync(id, cambiarEstadoDto.NuevoEstado);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al cambiar estado: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de compras
        /// </summary>
        [HttpGet("estadisticas")]
        public async Task<ActionResult<CompraStatsDto>> GetEstadisticas()
        {
            try
            {
                var stats = await _compraService.ObtenerEstadisticasAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener estadísticas: {ex.Message}" });
            }
        }

        /// <summary>
        /// Genera un nuevo número de compra
        /// </summary>
        [HttpGet("generar-numero")]
        public async Task<ActionResult<string>> GenerarNumero()
        {
            try
            {
                var numero = await _compraService.GenerarNumeroCompraAsync();
                return Ok(new { numeroCompra = numero });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al generar número: {ex.Message}" });
            }
        }
    }

    // DTO auxiliar para cambiar estado
    public class CambiarEstadoDto
    {
        public string NuevoEstado { get; set; } = string.Empty;
    }
}