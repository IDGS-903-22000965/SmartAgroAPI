using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAgro.API.Services;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MateriaPrimaController : ControllerBase
    {
        private readonly IMateriaPrimaService _materiaPrimaService;

        public MateriaPrimaController(IMateriaPrimaService materiaPrimaService)
        {
            _materiaPrimaService = materiaPrimaService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerMateriasPrimas()
        {
            try
            {
                var materiasPrimas = await _materiaPrimaService.ObtenerMateriasPrimasAsync();
                return Ok(new { success = true, data = materiasPrimas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener las materias primas",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerMateriaPrimaPorId(int id)
        {
            try
            {
                var materiaPrima = await _materiaPrimaService.ObtenerMateriaPrimaPorIdAsync(id);
                if (materiaPrima == null)
                    return NotFound(new { success = false, message = "Materia prima no encontrada" });

                return Ok(new { success = true, data = materiaPrima });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener la materia prima",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearMateriaPrima([FromBody] MateriaPrima materiaPrima)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var nuevaMateriaPrima = await _materiaPrimaService.CrearMateriaPrimaAsync(materiaPrima);
                return Ok(new
                {
                    success = true,
                    message = "Materia prima creada exitosamente",
                    data = nuevaMateriaPrima
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear la materia prima",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarMateriaPrima(int id, [FromBody] MateriaPrima materiaPrima)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                materiaPrima.Id = id;
                var resultado = await _materiaPrimaService.ActualizarMateriaPrimaAsync(materiaPrima);
                if (!resultado)
                    return NotFound(new { success = false, message = "Materia prima no encontrada" });

                return Ok(new
                {
                    success = true,
                    message = "Materia prima actualizada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al actualizar la materia prima",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarMateriaPrima(int id)
        {
            try
            {
                var resultado = await _materiaPrimaService.EliminarMateriaPrimaAsync(id);
                if (!resultado)
                    return NotFound(new { success = false, message = "Materia prima no encontrada" });

                return Ok(new
                {
                    success = true,
                    message = "Materia prima eliminada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al eliminar la materia prima",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}/stock")]
        public async Task<IActionResult> ActualizarStock(int id, [FromBody] int nuevoStock)
        {
            try
            {
                var resultado = await _materiaPrimaService.ActualizarStockAsync(id, nuevoStock);
                if (!resultado)
                    return NotFound(new { success = false, message = "Materia prima no encontrada" });

                return Ok(new
                {
                    success = true,
                    message = "Stock actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al actualizar el stock",
                    error = ex.Message
                });
            }
        }
    }
}