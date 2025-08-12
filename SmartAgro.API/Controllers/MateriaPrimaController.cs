using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.API.Services;
using SmartAgro.Data;
using SmartAgro.Models.DTOs;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MateriaPrimaController : ControllerBase
    {
        private readonly SmartAgroDbContext _context;
        private readonly IMateriaPrimaService _materiaPrimaService;
        private readonly ICosteoFifoService _costeoFifoService;

        public MateriaPrimaController(
            SmartAgroDbContext context,
            IMateriaPrimaService materiaPrimaService,
            ICosteoFifoService costeoFifoService)
        {
            _context = context;
            _materiaPrimaService = materiaPrimaService;
            _costeoFifoService = costeoFifoService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerMateriasPrimas()
        {
            try
            {
                // ✅ CONSULTA DIRECTA OPTIMIZADA CON DTOs
                var materiasPrimas = await _context.MateriasPrimas
                    .Include(m => m.Proveedor)
                    .Where(m => m.Activo) // Solo materias primas activas
                    .Select(m => new
                    {
                        Id = m.Id,
                        Nombre = m.Nombre,
                        Descripcion = m.Descripcion,
                        UnidadMedida = m.UnidadMedida,
                        CostoUnitario = m.CostoUnitario,
                        Stock = m.Stock,
                        StockMinimo = m.StockMinimo,
                        Activo = m.Activo,
                        ProveedorId = m.ProveedorId,
                        ProveedorNombre = m.Proveedor.Nombre,
                        ValorInventario = m.Stock * m.CostoUnitario,
                        FechaCreacion = DateTime.Now, // Puedes agregar esta columna a la BD si la necesitas
                        FechaActualizacion = DateTime.Now
                    })
                    .OrderBy(m => m.Nombre)
                    .ToListAsync();

                Console.WriteLine($"✅ Materias primas encontradas: {materiasPrimas.Count}");

                return Ok(new { success = true, data = materiasPrimas });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
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
                var materiaPrima = await _context.MateriasPrimas
                    .Include(m => m.Proveedor)
                    .Where(m => m.Id == id)
                    .Select(m => new
                    {
                        Id = m.Id,
                        Nombre = m.Nombre,
                        Descripcion = m.Descripcion,
                        UnidadMedida = m.UnidadMedida,
                        CostoUnitario = m.CostoUnitario,
                        Stock = m.Stock,
                        StockMinimo = m.StockMinimo,
                        Activo = m.Activo,
                        ProveedorId = m.ProveedorId,
                        ProveedorNombre = m.Proveedor.Nombre,
                        ValorInventario = m.Stock * m.CostoUnitario
                    })
                    .FirstOrDefaultAsync();

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
        public async Task<IActionResult> CrearMateriaPrima([FromBody] MateriaPrimaCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Datos inválidos",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                // Verificar que el proveedor existe
                var proveedorExiste = await _context.Proveedores
                    .AnyAsync(p => p.Id == dto.ProveedorId && p.Activo);

                if (!proveedorExiste)
                {
                    return BadRequest(new { success = false, message = "El proveedor seleccionado no existe o no está activo" });
                }

                var materiaPrima = new MateriaPrima
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    UnidadMedida = dto.UnidadMedida,
                    CostoUnitario = dto.CostoUnitario,
                    Stock = dto.Stock,
                    StockMinimo = dto.StockMinimo,
                    ProveedorId = dto.ProveedorId,
                    Activo = true
                };

                _context.MateriasPrimas.Add(materiaPrima);
                await _context.SaveChangesAsync();

                // Retornar la materia prima creada con datos del proveedor
                var materiaPrimaCreada = await _context.MateriasPrimas
                    .Include(m => m.Proveedor)
                    .Where(m => m.Id == materiaPrima.Id)
                    .Select(m => new
                    {
                        Id = m.Id,
                        Nombre = m.Nombre,
                        Descripcion = m.Descripcion,
                        UnidadMedida = m.UnidadMedida,
                        CostoUnitario = m.CostoUnitario,
                        Stock = m.Stock,
                        StockMinimo = m.StockMinimo,
                        Activo = m.Activo,
                        ProveedorId = m.ProveedorId,
                        ProveedorNombre = m.Proveedor.Nombre,
                        ValorInventario = m.Stock * m.CostoUnitario
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = "Materia prima creada exitosamente",
                    data = materiaPrimaCreada
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
        public async Task<IActionResult> ActualizarMateriaPrima(int id, [FromBody] MateriaPrimaUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var materiaPrimaExistente = await _context.MateriasPrimas.FindAsync(id);
                if (materiaPrimaExistente == null)
                    return NotFound(new { success = false, message = "Materia prima no encontrada" });

                // Verificar que el proveedor existe
                var proveedorExiste = await _context.Proveedores
                    .AnyAsync(p => p.Id == dto.ProveedorId && p.Activo);

                if (!proveedorExiste)
                {
                    return BadRequest(new { success = false, message = "El proveedor seleccionado no existe o no está activo" });
                }

                // Actualizar campos
                materiaPrimaExistente.Nombre = dto.Nombre;
                materiaPrimaExistente.Descripcion = dto.Descripcion;
                materiaPrimaExistente.UnidadMedida = dto.UnidadMedida;
                materiaPrimaExistente.CostoUnitario = dto.CostoUnitario;
                materiaPrimaExistente.Stock = dto.Stock;
                materiaPrimaExistente.StockMinimo = dto.StockMinimo;
                materiaPrimaExistente.ProveedorId = dto.ProveedorId;
                materiaPrimaExistente.Activo = dto.Activo;

                await _context.SaveChangesAsync();

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
                var materiaPrima = await _context.MateriasPrimas.FindAsync(id);
                if (materiaPrima == null)
                    return NotFound(new { success = false, message = "Materia prima no encontrada" });

                // Verificar si está siendo usada en productos
                var estaEnUso = await _context.Set<ProductoMateriaPrima>()
                    .AnyAsync(pm => pm.MateriaPrimaId == id);

                if (estaEnUso)
                {
                    // Solo desactivar en lugar de eliminar
                    materiaPrima.Activo = false;
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Materia prima desactivada (estaba en uso en productos)"
                    });
                }
                else
                {
                    // Eliminar completamente
                    _context.MateriasPrimas.Remove(materiaPrima);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Materia prima eliminada exitosamente"
                    });
                }
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

        // Resto de métodos existentes...
        [HttpGet("{id}/movimientos")]
        public async Task<IActionResult> ObtenerMovimientosStock(int id)
        {
            try
            {
                var materiaPrimaExiste = await _context.MateriasPrimas.AnyAsync(m => m.Id == id);
                if (!materiaPrimaExiste)
                    return NotFound(new { success = false, message = "Materia prima no encontrada" });

                var movimientos = await _context.MovimientosStock
                    .Where(m => m.MateriaPrimaId == id)
                    .Include(m => m.MateriaPrima)
                    .OrderByDescending(m => m.Fecha)
                    .Select(m => new MovimientoStockResponseDto
                    {
                        Id = m.Id,
                        MateriaPrimaId = m.MateriaPrimaId,
                        MateriaPrimaNombre = m.MateriaPrima.Nombre,
                        Tipo = m.Tipo,
                        Cantidad = m.Cantidad,
                        CostoUnitario = m.CostoUnitario,
                        Fecha = m.Fecha,
                        Referencia = m.Referencia,
                        Observaciones = m.Observaciones
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = movimientos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener movimientos de stock",
                    error = ex.Message
                });
            }
        }

        [HttpGet("bajo-stock")]
        public async Task<IActionResult> ObtenerBajoStock()
        {
            try
            {
                var materiasBajoStock = await _context.MateriasPrimas
                    .Include(m => m.Proveedor)
                    .Where(m => m.Activo && m.Stock <= m.StockMinimo)
                    .Select(m => new
                    {
                        Id = m.Id,
                        Nombre = m.Nombre,
                        Stock = m.Stock,
                        StockMinimo = m.StockMinimo,
                        ProveedorNombre = m.Proveedor.Nombre,
                        UnidadMedida = m.UnidadMedida
                    })
                    .OrderBy(m => m.Stock)
                    .ToListAsync();

                return Ok(new { success = true, data = materiasBajoStock });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener materias primas bajo stock",
                    error = ex.Message
                });
            }
        }
    }
}