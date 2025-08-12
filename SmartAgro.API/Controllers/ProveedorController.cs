using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProveedorController : ControllerBase
    {
        private readonly SmartAgroDbContext _context;

        public ProveedorController(SmartAgroDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProveedores()
        {
            try
            {
                var proveedores = await _context.Proveedores
                    .Where(p => p.Activo)
                    .Select(p => new
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        RazonSocial = p.RazonSocial,
                        RFC = p.RFC,
                        Email = p.Email,
                        Telefono = p.Telefono,
                        Direccion = p.Direccion,
                        ContactoPrincipal = p.ContactoPrincipal,
                        Activo = p.Activo,
                        FechaRegistro = p.FechaRegistro,
                        CantidadMateriasPrimas = p.MateriasPrimas.Count(m => m.Activo)
                    })
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                Console.WriteLine($"✅ Proveedores encontrados: {proveedores.Count}");

                return Ok(new { success = true, data = proveedores });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
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
                var proveedor = await _context.Proveedores
                    .Where(p => p.Id == id)
                    .Select(p => new
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        RazonSocial = p.RazonSocial,
                        RFC = p.RFC,
                        Email = p.Email,
                        Telefono = p.Telefono,
                        Direccion = p.Direccion,
                        ContactoPrincipal = p.ContactoPrincipal,
                        Activo = p.Activo,
                        FechaRegistro = p.FechaRegistro,
                        MateriasPrimas = p.MateriasPrimas
                            .Where(m => m.Activo)
                            .Select(m => new
                            {
                                Id = m.Id,
                                Nombre = m.Nombre,
                                UnidadMedida = m.UnidadMedida,
                                Stock = m.Stock,
                                CostoUnitario = m.CostoUnitario
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

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
                // Verificar que no exista un proveedor con el mismo RFC
                if (!string.IsNullOrEmpty(proveedor.RFC))
                {
                    var rfcExiste = await _context.Proveedores
                        .AnyAsync(p => p.RFC == proveedor.RFC && p.Activo);

                    if (rfcExiste)
                    {
                        return BadRequest(new { success = false, message = "Ya existe un proveedor con ese RFC" });
                    }
                }

                proveedor.FechaRegistro = DateTime.Now;
                proveedor.Activo = true;

                _context.Proveedores.Add(proveedor);
                await _context.SaveChangesAsync();

                var proveedorCreado = await _context.Proveedores
                    .Where(p => p.Id == proveedor.Id)
                    .Select(p => new
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        RazonSocial = p.RazonSocial,
                        RFC = p.RFC,
                        Email = p.Email,
                        Telefono = p.Telefono,
                        Direccion = p.Direccion,
                        ContactoPrincipal = p.ContactoPrincipal,
                        Activo = p.Activo,
                        FechaRegistro = p.FechaRegistro,
                        CantidadMateriasPrimas = 0
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = "Proveedor creado exitosamente",
                    data = proveedorCreado
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
                var proveedorExistente = await _context.Proveedores.FindAsync(id);
                if (proveedorExistente == null)
                    return NotFound(new { success = false, message = "Proveedor no encontrado" });

                // Verificar RFC único (excluyendo el proveedor actual)
                if (!string.IsNullOrEmpty(proveedor.RFC))
                {
                    var rfcExiste = await _context.Proveedores
                        .AnyAsync(p => p.RFC == proveedor.RFC && p.Id != id && p.Activo);

                    if (rfcExiste)
                    {
                        return BadRequest(new { success = false, message = "Ya existe otro proveedor con ese RFC" });
                    }
                }

                // Actualizar campos
                proveedorExistente.Nombre = proveedor.Nombre;
                proveedorExistente.RazonSocial = proveedor.RazonSocial;
                proveedorExistente.RFC = proveedor.RFC;
                proveedorExistente.Email = proveedor.Email;
                proveedorExistente.Telefono = proveedor.Telefono;
                proveedorExistente.Direccion = proveedor.Direccion;
                proveedorExistente.ContactoPrincipal = proveedor.ContactoPrincipal;
                proveedorExistente.Activo = proveedor.Activo;

                await _context.SaveChangesAsync();

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
                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor == null)
                    return NotFound(new { success = false, message = "Proveedor no encontrado" });

                // Verificar si tiene materias primas asociadas
                var tieneMateriasPrimas = await _context.MateriasPrimas
                    .AnyAsync(m => m.ProveedorId == id);

                if (tieneMateriasPrimas)
                {
                    // Solo desactivar en lugar de eliminar
                    proveedor.Activo = false;
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Proveedor desactivado (tiene materias primas asociadas)"
                    });
                }
                else
                {
                    // Eliminar completamente
                    _context.Proveedores.Remove(proveedor);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Proveedor eliminado exitosamente"
                    });
                }
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