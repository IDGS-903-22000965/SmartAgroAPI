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
        public async Task<IActionResult> ActualizarMateriaPrima(int id, [FromBody] MateriaPrimaUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var materiaPrima = new MateriaPrima
                {
                    Id = id,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    UnidadMedida = dto.UnidadMedida,
                    CostoUnitario = dto.CostoUnitario,
                    Stock = dto.Stock,
                    StockMinimo = dto.StockMinimo,
                    ProveedorId = dto.ProveedorId,
                    Activo = dto.Activo
                };

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

        // ✅ MÉTODO CORREGIDO PARA OBTENER MOVIMIENTOS REALES
        [HttpGet("{id}/movimientos")]
        public async Task<IActionResult> ObtenerMovimientosStock(int id)
        {
            try
            {
                // Verificar que la materia prima existe
                var materiaPrima = await _materiaPrimaService.ObtenerMateriaPrimaPorIdAsync(id);
                if (materiaPrima == null)
                    return NotFound(new { success = false, message = "Materia prima no encontrada" });

                // Obtener movimientos reales de la base de datos
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

        [HttpPost("movimientos")]
        public async Task<IActionResult> RegistrarMovimientoStock([FromBody] MovimientoStockDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var materiaPrima = await _materiaPrimaService.ObtenerMateriaPrimaPorIdAsync(dto.MateriaPrimaId);
                if (materiaPrima == null)
                    return NotFound(new { success = false, message = "Materia prima no encontrada" });

                // Calcular nuevo stock según el tipo de movimiento
                var nuevoStock = dto.Tipo switch
                {
                    "Entrada" => materiaPrima.Stock + (int)dto.Cantidad,
                    "Salida" => Math.Max(0, materiaPrima.Stock - (int)dto.Cantidad),
                    "Ajuste" => (int)dto.Cantidad,
                    _ => materiaPrima.Stock
                };

                // Validar que no se genere stock negativo en salidas
                if (dto.Tipo == "Salida" && materiaPrima.Stock < (int)dto.Cantidad)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"No hay suficiente stock disponible. Stock actual: {materiaPrima.Stock}"
                    });
                }

                // Registrar el movimiento en la tabla MovimientosStock
                var movimiento = new MovimientoStock
                {
                    MateriaPrimaId = dto.MateriaPrimaId,
                    Tipo = dto.Tipo,
                    Cantidad = dto.Cantidad,
                    CostoUnitario = dto.CostoUnitario,
                    Referencia = dto.Referencia,
                    Observaciones = dto.Observaciones,
                    Fecha = DateTime.Now
                };

                _context.MovimientosStock.Add(movimiento);

                // Actualizar stock de la materia prima
                await _materiaPrimaService.ActualizarStockAsync(dto.MateriaPrimaId, nuevoStock);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Movimiento de stock registrado exitosamente",
                    nuevoStock = nuevoStock,
                    movimiento = new MovimientoStockResponseDto
                    {
                        Id = movimiento.Id,
                        MateriaPrimaId = movimiento.MateriaPrimaId,
                        MateriaPrimaNombre = materiaPrima.Nombre,
                        Tipo = movimiento.Tipo,
                        Cantidad = movimiento.Cantidad,
                        CostoUnitario = movimiento.CostoUnitario,
                        Fecha = movimiento.Fecha,
                        Referencia = movimiento.Referencia,
                        Observaciones = movimiento.Observaciones
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al registrar movimiento de stock",
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

        [HttpGet("proveedor/{proveedorId}")]
        public async Task<IActionResult> ObtenerPorProveedor(int proveedorId)
        {
            try
            {
                var materiasPrimas = await _materiaPrimaService.ObtenerPorProveedorAsync(proveedorId);
                return Ok(new { success = true, data = materiasPrimas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener materias primas del proveedor",
                    error = ex.Message
                });
            }
        }

        [HttpGet("bajo-stock")]
        public async Task<IActionResult> ObtenerBajoStock()
        {
            try
            {
                var materiasPrimas = await _materiaPrimaService.ObtenerBajoStockAsync();
                return Ok(new { success = true, data = materiasPrimas });
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

        [HttpGet("{id}/costeo/fifo")]
        public async Task<IActionResult> CalcularCostoFifo(int id, [FromQuery] decimal cantidad)
        {
            try
            {
                var costo = await _costeoFifoService.ObtenerCostoSalidaFifoAsync(id, cantidad);
                return Ok(new { success = true, costo });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}/costeo/promedio")]
        public async Task<IActionResult> CalcularCosteoPromedio(int id)
        {
            try
            {
                var materiaPrima = await _materiaPrimaService.ObtenerMateriaPrimaPorIdAsync(id);
                if (materiaPrima == null)
                    return NotFound(new { success = false, message = "Materia prima no encontrada" });

                // Calcular costo promedio ponderado real
                var entradas = await _context.MovimientosStock
                    .Where(m => m.MateriaPrimaId == id && m.Tipo == "Entrada")
                    .ToListAsync();

                var costoPromedio = entradas.Any()
                    ? entradas.Sum(e => e.Cantidad * e.CostoUnitario) / entradas.Sum(e => e.Cantidad)
                    : materiaPrima.CostoUnitario;

                var resultado = new
                {
                    CostoActual = materiaPrima.CostoUnitario,
                    CostoPromedio = costoPromedio,
                    FechaCalculo = DateTime.Now,
                    MetodoCalculo = "Promedio Ponderado",
                    TotalEntradas = entradas.Count,
                    CantidadTotal = entradas.Sum(e => e.Cantidad)
                };

                return Ok(new { success = true, data = resultado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al calcular costeo promedio",
                    error = ex.Message
                });
            }
        }
    }
}