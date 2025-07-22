using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAgro.API.Services;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProveedorController : ControllerBase
    {
        private readonly IProveedorService _proveedorService;

        public ProveedorController(IProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProveedores()
        {
            try
            {
                var proveedores = await _proveedorService.ObtenerProveedoresAsync();
                return Ok(new { success = true, data = proveedores });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener los proveedores",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerProveedorPorId(int id)
        {
            try
            {
                var proveedor = await _proveedorService.ObtenerProveedorPorIdAsync(id);
                if (proveedor == null)
                    return NotFound(new { success = false, message = "Proveedor no encontrado" });

                return Ok(new { success = true, data = proveedor });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener el proveedor",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearProveedor([FromBody] Proveedor proveedor)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var nuevoProveedor = await _proveedorService.CrearProveedorAsync(proveedor);
                return Ok(new
                {
                    success = true,
                    message = "Proveedor creado exitosamente",
                    data = nuevoProveedor
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear el proveedor",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarProveedor(int id, [FromBody] Proveedor proveedor)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                proveedor.Id = id;
                var resultado = await _proveedorService.ActualizarProveedorAsync(proveedor);
                if (!resultado)
                    return NotFound(new { success = false, message = "Proveedor no encontrado" });

                return Ok(new
                {
                    success = true,
                    message = "Proveedor actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al actualizar el proveedor",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProveedor(int id)
        {
            try
            {
                var resultado = await _proveedorService.EliminarProveedorAsync(id);
                if (!resultado)
                    return NotFound(new { success = false, message = "Proveedor no encontrado" });

                return Ok(new
                {
                    success = true,
                    message = "Proveedor eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al eliminar el proveedor",
                    error = ex.Message
                });
            }
        }
    }
}
