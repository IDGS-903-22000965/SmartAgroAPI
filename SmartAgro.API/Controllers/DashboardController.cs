// SmartAgro.API/Controllers/DashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;

namespace SmartAgro.API.Controllers
{
    // DTO para actividades
    public class ActividadDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    // DTO para alertas
    public class AlertaDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly SmartAgroDbContext _context;

        public DashboardController(SmartAgroDbContext context)
        {
            _context = context;
        }

        [HttpGet("metricas")]
        public async Task<IActionResult> ObtenerMetricas()
        {
            try
            {
                // Fechas para cálculos
                var hoy = DateTime.Today;
                var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
                var inicioMesAnterior = inicioMes.AddMonths(-1);
                var finMesAnterior = inicioMes.AddDays(-1);

                // Ventas
                var ventasHoy = await _context.Ventas
                    .Where(v => v.FechaVenta.Date == hoy)
                    .SumAsync(v => (decimal?)v.Total) ?? 0;

                var ventasEsteMes = await _context.Ventas
                    .Where(v => v.FechaVenta >= inicioMes)
                    .SumAsync(v => (decimal?)v.Total) ?? 0;

                var ventasMesAnterior = await _context.Ventas
                    .Where(v => v.FechaVenta >= inicioMesAnterior && v.FechaVenta <= finMesAnterior)
                    .SumAsync(v => (decimal?)v.Total) ?? 0;

                // Cotizaciones
                var cotizacionesHoy = await _context.Cotizaciones
                    .CountAsync(c => c.FechaCotizacion.Date == hoy);

                var cotizacionesPendientes = await _context.Cotizaciones
                    .CountAsync(c => c.Estado == "Pendiente");

                var cotizacionesAprobadas = await _context.Cotizaciones
                    .CountAsync(c => c.Estado == "Aprobada");

                // Compras a proveedores
                var comprasEsteMes = await _context.ComprasProveedores
                    .Where(c => c.FechaCompra >= inicioMes)
                    .SumAsync(c => (decimal?)c.Total) ?? 0;

                var comprasPendientes = await _context.ComprasProveedores
                    .CountAsync(c => c.Estado == "Pendiente");

                // Productos y materias primas
                var totalProductos = await _context.Productos
                    .CountAsync(p => p.Activo);

                var materiasPrimasBajoStock = await _context.MateriasPrimas
                    .CountAsync(m => m.Activo && m.Stock <= m.StockMinimo);

                // Calcular porcentaje de cambio
                var porcentajeCambioVentas = ventasMesAnterior > 0
                    ? ((ventasEsteMes - ventasMesAnterior) / ventasMesAnterior) * 100
                    : 0;

                var metricas = new
                {
                    ventas = new
                    {
                        hoy = ventasHoy,
                        esteMes = ventasEsteMes,
                        mesAnterior = ventasMesAnterior,
                        porcentajeCambio = Math.Round(porcentajeCambioVentas, 2)
                    },
                    cotizaciones = new
                    {
                        hoy = cotizacionesHoy,
                        pendientes = cotizacionesPendientes,
                        aprobadas = cotizacionesAprobadas
                    },
                    compras = new
                    {
                        esteMes = comprasEsteMes,
                        pendientes = comprasPendientes
                    },
                    inventario = new
                    {
                        totalProductos = totalProductos,
                        materiasPrimasBajoStock = materiasPrimasBajoStock
                    }
                };

                return Ok(metricas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener métricas del dashboard",
                    error = ex.Message
                });
            }
        }

        [HttpGet("actividad-reciente")]
        public async Task<IActionResult> ObtenerActividadReciente()
        {
            try
            {
                var actividades = new List<ActividadDto>();
                var fecha = DateTime.Now.AddDays(-7); // Últimos 7 días

                // Ventas recientes
                var ventasRecientes = await _context.Ventas
                    .Where(v => v.FechaVenta >= fecha)
                    .OrderByDescending(v => v.FechaVenta)
                    .Take(5)
                    .Select(v => new ActividadDto
                    {
                        Tipo = "Venta",
                        Descripcion = $"Venta #{v.NumeroVenta} por ${v.Total:F2}",
                        Fecha = v.FechaVenta,
                        Estado = v.EstadoVenta ?? "Pendiente"
                    })
                    .ToListAsync();

                actividades.AddRange(ventasRecientes);

                // Cotizaciones recientes
                var cotizacionesRecientes = await _context.Cotizaciones
                    .Where(c => c.FechaCotizacion >= fecha)
                    .OrderByDescending(c => c.FechaCotizacion)
                    .Take(5)
                    .Select(c => new ActividadDto
                    {
                        Tipo = "Cotización",
                        Descripcion = $"Cotización #{c.NumeroCotizacion} para {c.NombreCliente}",
                        Fecha = c.FechaCotizacion,
                        Estado = c.Estado ?? "Pendiente"
                    })
                    .ToListAsync();

                actividades.AddRange(cotizacionesRecientes);

                // Compras recientes
                var comprasRecientes = await _context.ComprasProveedores
                    .Include(c => c.Proveedor)
                    .Where(c => c.FechaCompra >= fecha)
                    .OrderByDescending(c => c.FechaCompra)
                    .Take(5)
                    .Select(c => new ActividadDto
                    {
                        Tipo = "Compra",
                        Descripcion = $"Compra #{c.NumeroCompra} a {c.Proveedor.Nombre}",
                        Fecha = c.FechaCompra,
                        Estado = c.Estado ?? "Pendiente"
                    })
                    .ToListAsync();

                actividades.AddRange(comprasRecientes);

                // Ordenar por fecha más reciente
                var actividadesOrdenadas = actividades
                    .OrderByDescending(a => a.Fecha)
                    .Take(10)
                    .ToList();

                return Ok(actividadesOrdenadas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener actividad reciente",
                    error = ex.Message
                });
            }
        }

        [HttpGet("estadisticas-mensuales")]
        public async Task<IActionResult> ObtenerEstadisticasMensuales()
        {
            try
            {
                var estadisticas = new List<object>();

                // Últimos 12 meses
                for (int i = 11; i >= 0; i--)
                {
                    var fecha = DateTime.Now.AddMonths(-i);
                    var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
                    var finMes = inicioMes.AddMonths(1).AddDays(-1);

                    var ventasMes = await _context.Ventas
                        .Where(v => v.FechaVenta >= inicioMes && v.FechaVenta <= finMes)
                        .SumAsync(v => (decimal?)v.Total) ?? 0;

                    var cotizacionesMes = await _context.Cotizaciones
                        .CountAsync(c => c.FechaCotizacion >= inicioMes && c.FechaCotizacion <= finMes);

                    var comprasMes = await _context.ComprasProveedores
                        .Where(c => c.FechaCompra >= inicioMes && c.FechaCompra <= finMes)
                        .SumAsync(c => (decimal?)c.Total) ?? 0;

                    estadisticas.Add(new
                    {
                        mes = fecha.ToString("MMM yyyy"),
                        ventas = ventasMes,
                        cotizaciones = cotizacionesMes,
                        compras = comprasMes
                    });
                }

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener estadísticas mensuales",
                    error = ex.Message
                });
            }
        }

        [HttpGet("productos-mas-vendidos")]
        public async Task<IActionResult> ObtenerProductosMasVendidos()
        {
            try
            {
                var fecha = DateTime.Now.AddMonths(-3); // Últimos 3 meses

                var productos = await _context.DetallesVenta
                    .Include(d => d.Producto)
                    .Include(d => d.Venta)
                    .Where(d => d.Venta.FechaVenta >= fecha)
                    .GroupBy(d => new { d.ProductoId, d.Producto.Nombre })
                    .Select(g => new
                    {
                        ProductoId = g.Key.ProductoId,
                        NombreProducto = g.Key.Nombre,
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        TotalVentas = g.Sum(d => d.Subtotal)
                    })
                    .OrderByDescending(p => p.CantidadVendida)
                    .Take(10)
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener productos más vendidos",
                    error = ex.Message
                });
            }
        }

        [HttpGet("alertas")]
        public async Task<IActionResult> ObtenerAlertas()
        {
            try
            {
                var alertas = new List<AlertaDto>();

                // Materias primas bajo stock
                var materiasBajoStock = await _context.MateriasPrimas
                    .Include(m => m.Proveedor)
                    .Where(m => m.Activo && m.Stock <= m.StockMinimo)
                    .Select(m => new AlertaDto
                    {
                        Tipo = "stock",
                        Titulo = "Stock bajo",
                        Descripcion = $"{m.Nombre} - Stock: {m.Stock} (Mínimo: {m.StockMinimo})",
                        Prioridad = "alta",
                        Fecha = DateTime.Now
                    })
                    .ToListAsync();

                alertas.AddRange(materiasBajoStock);

                // Cotizaciones próximas a vencer
                var fechaLimite = DateTime.Now.AddDays(3);
                var cotizacionesPorVencer = await _context.Cotizaciones
                    .Where(c => c.Estado == "Pendiente" && c.FechaVencimiento <= fechaLimite)
                    .Select(c => new AlertaDto
                    {
                        Tipo = "cotizacion",
                        Titulo = "Cotización por vencer",
                        Descripcion = $"Cotización #{c.NumeroCotizacion} vence el {c.FechaVencimiento:dd/MM/yyyy}",
                        Prioridad = "media",
                        Fecha = c.FechaVencimiento
                    })
                    .ToListAsync();

                alertas.AddRange(cotizacionesPorVencer);

                // Compras pendientes
                var comprasPendientes = await _context.ComprasProveedores
                    .Include(c => c.Proveedor)
                    .Where(c => c.Estado == "Pendiente" && c.FechaCompra <= DateTime.Now.AddDays(-7))
                    .Select(c => new AlertaDto
                    {
                        Tipo = "compra",
                        Titulo = "Compra pendiente",
                        Descripcion = $"Compra #{c.NumeroCompra} a {c.Proveedor.Nombre} lleva {(DateTime.Now - c.FechaCompra).Days} días pendiente",
                        Prioridad = "media",
                        Fecha = c.FechaCompra
                    })
                    .ToListAsync();

                alertas.AddRange(comprasPendientes);

                return Ok(alertas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener alertas",
                    error = ex.Message
                });
            }
        }
    }
}