// SmartAgro.API/Controllers/MateriaPrimaController.cs
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
        private readonly ICosteoFifoService _costeoFifoService;

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
        // Agregar estos métodos al MateriaPrimaController existente

[HttpGet("{id}/movimientos")]
public async Task<IActionResult> ObtenerMovimientosStock(int id)
{
    try
    {
        // Aquí implementarías la lógica para obtener movimientos de stock
        // Por ahora retorno datos de ejemplo
        var movimientos = new[]
        {
            new
            {
                Id = 1,
                Tipo = "Entrada",
                Cantidad = 100,
                CostoUnitario = 45.50m,
                Fecha = DateTime.Now.AddDays(-30),
                Referencia = "CP-202401-001",
                Observaciones = "Compra inicial"
            },
            new
            {
                Id = 2,
                Tipo = "Salida",
                Cantidad = 25,
                CostoUnitario = 45.50m,
                Fecha = DateTime.Now.AddDays(-15),
                Referencia = "PROD-001",
                Observaciones = "Uso en producción"
            }
        };

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
public async Task<IActionResult> RegistrarMovimientoStock([FromBody] MovimientoStockDto movimiento)
{
    try
    {
        // Aquí implementarías la lógica para registrar un movimiento de stock
        // Esto incluiría:
        // 1. Validar que la materia prima existe
        // 2. Calcular el nuevo stock según el tipo de movimiento
        // 3. Actualizar el costo promedio si es necesario
        // 4. Registrar el movimiento en el historial

        var materiaPrima = await _materiaPrimaService.ObtenerMateriaPrimaPorIdAsync(movimiento.MateriaPrimaId);
        if (materiaPrima == null)
            return NotFound(new { success = false, message = "Materia prima no encontrada" });

        // Simular actualización de stock
        var nuevoStock = movimiento.Tipo switch
        {
            "Entrada" => materiaPrima.Stock + movimiento.Cantidad,
            "Salida" => Math.Max(0, materiaPrima.Stock - movimiento.Cantidad),
            "Ajuste" => movimiento.Cantidad,
            _ => materiaPrima.Stock
        };

        // Aquí actualizarías el stock y registrarías el movimiento
        await _materiaPrimaService.ActualizarStockAsync(movimiento.MateriaPrimaId, (int)nuevoStock);

        return Ok(new
        {
            success = true,
            message = "Movimiento de stock registrado exitosamente",
            nuevoStock = nuevoStock
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

[HttpGet("{id}/costeo/promedio")]
public async Task<IActionResult> CalcularCosteoPromedio(int id)
{
    try
    {
        var materiaPrima = await _materiaPrimaService.ObtenerMateriaPrimaPorIdAsync(id);
        if (materiaPrima == null)
            return NotFound(new { success = false, message = "Materia prima no encontrada" });

        // Implementar cálculo de costo promedio ponderado
        // Este es un ejemplo simplificado
        var costoPromedio = new
        {
            CostoActual = materiaPrima.CostoUnitario,
            CostoPromedio = materiaPrima.CostoUnitario, // Aquí implementarías el cálculo real
            FechaCalculo = DateTime.Now,
            MetodoCalculo = "Promedio Ponderado"
        };

        return Ok(new { success = true, data = costoPromedio });
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

[HttpGet("{id}/costeo/ultimo")]
public async Task<IActionResult> CalcularCosteoUltimo(int id)
{
    try
    {
        var materiaPrima = await _materiaPrimaService.ObtenerMateriaPrimaPorIdAsync(id);
        if (materiaPrima == null)
            return NotFound(new { success = false, message = "Materia prima no encontrada" });

        // Implementar cálculo de último costo (LIFO)
        var costoUltimo = new
        {
            CostoActual = materiaPrima.CostoUnitario,
            UltimoCosto = materiaPrima.CostoUnitario, // Aquí implementarías el cálculo real
            FechaUltimaEntrada = DateTime.Now.AddDays(-5),
            MetodoCalculo = "Último Costo (LIFO)"
        };

        return Ok(new { success = true, data = costoUltimo });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            success = false,
            message = "Error al calcular último costo",
            error = ex.Message
        });
    }
}

[HttpGet("reporte-costeo")]
public async Task<IActionResult> ObtenerReporteCosteo(
    [FromQuery] int? materiaPrimaId = null,
    [FromQuery] DateTime? fechaInicio = null,
    [FromQuery] DateTime? fechaFin = null)
{
    try
    {
                var materiasPrimas = materiaPrimaId.HasValue
            ? new List<MateriaPrima> { await _materiaPrimaService.ObtenerMateriaPrimaPorIdAsync(materiaPrimaId.Value) }
            : await _materiaPrimaService.ObtenerMateriasPrimasAsync();


                var reporte = materiasPrimas.Where(m => m != null && m.Activo).Select(m => new
        {
            Id = m.Id,
            Nombre = m.Nombre,
            Stock = m.Stock,
            CostoUnitarioActual = m.CostoUnitario,
            ValorInventario = m.Stock * m.CostoUnitario,
            CostoPromedio = m.CostoUnitario, // Implementar cálculo real
            UltimoCosto = m.CostoUnitario,   // Implementar cálculo real
            UnidadMedida = m.UnidadMedida,
            Proveedor = m.Proveedor?.Nombre,
            FechaCalculo = DateTime.Now
        }).ToList();

        var resumen = new
        {
            TotalMateriasPrimas = reporte.Count,
            ValorTotalInventario = reporte.Sum(r => r.ValorInventario),
            FechaReporte = DateTime.Now,
            Detalles = reporte
        };

        return Ok(new { success = true, data = resumen });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            success = false,
            message = "Error al generar reporte de costeo",
            error = ex.Message
        });
    }
}

[HttpGet("valor-inventario")]
public async Task<IActionResult> ObtenerValorInventario()
{
    try
    {
        var materiasPrimas = await _materiaPrimaService.ObtenerMateriasPrimasAsync();
        
        var valorTotal = materiasPrimas
            .Where(m => m.Activo)
            .Sum(m => m.Stock * m.CostoUnitario);

        var estadisticas = new
        {
            ValorTotalInventario = valorTotal,
            TotalMateriasPrimas = materiasPrimas.Count(m => m.Activo),
            MateriasBajoStock = materiasPrimas.Count(m => m.Activo && m.Stock <= m.StockMinimo),
            MateriasSinStock = materiasPrimas.Count(m => m.Activo && m.Stock == 0),
            FechaCalculo = DateTime.Now
        };

        return Ok(new { success = true, data = estadisticas });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            success = false,
            message = "Error al calcular valor de inventario",
            error = ex.Message
        });
    }
}

[HttpGet("buscar")]
public async Task<IActionResult> BuscarMateriasPrimas(
    [FromQuery] string? busqueda = null,
    [FromQuery] int? proveedorId = null,
    [FromQuery] bool? activo = null,
    [FromQuery] bool bajoStock = false,
    [FromQuery] bool sinStock = false)
{
    try
    {
        var materiasPrimas = await _materiaPrimaService.ObtenerMateriasPrimasAsync();
        var query = materiasPrimas.AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrEmpty(busqueda))
        {
            var termino = busqueda.ToLower();
            query = query.Where(m => 
                m.Nombre.ToLower().Contains(termino) ||
                (m.Descripcion != null && m.Descripcion.ToLower().Contains(termino)));
        }

        if (proveedorId.HasValue)
        {
            query = query.Where(m => m.ProveedorId == proveedorId.Value);
        }

        if (activo.HasValue)
        {
            query = query.Where(m => m.Activo == activo.Value);
        }

        if (bajoStock)
        {
            query = query.Where(m => m.Stock <= m.StockMinimo);
        }

        if (sinStock)
        {
            query = query.Where(m => m.Stock == 0);
        }

        var resultado = query.ToList();

        return Ok(new { success = true, data = resultado });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            success = false,
            message = "Error en la búsqueda",
            error = ex.Message
        });
    }
}

// DTO para movimientos de stock
public class MovimientoStockDto
{
    public int MateriaPrimaId { get; set; }
    public string Tipo { get; set; } = string.Empty; // Entrada, Salida, Ajuste
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }
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
    }
}