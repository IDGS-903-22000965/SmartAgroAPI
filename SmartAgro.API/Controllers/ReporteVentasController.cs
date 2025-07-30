using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.API.Services;
using SmartAgro.Data;
using SmartAgro.Models.DTOs.Ventas;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReporteVentasController : ControllerBase
    {
        private readonly IVentaService _ventaService;
        private readonly ILogger<ReporteVentasController> _logger;
        private readonly SmartAgroDbContext _context;

        public ReporteVentasController(
            IVentaService ventaService,
            ILogger<ReporteVentasController> logger,
            SmartAgroDbContext context)
        {
            _ventaService = ventaService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Obtiene el dashboard principal de ventas
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<object>> GetDashboardVentas()
        {
            try
            {
                var estadisticas = await _ventaService.ObtenerEstadisticasVentasAsync();
                var ventasPorEstado = await _ventaService.ObtenerVentasPorEstadoAsync();
                var ventasPorMetodoPago = await _ventaService.ObtenerVentasPorMetodoPagoAsync();
                var productosMasVendidos = await _ventaService.ObtenerProductosMasVendidosAsync(top: 5);

                var dashboard = new
                {
                    estadisticas,
                    ventasPorEstado,
                    ventasPorMetodoPago,
                    productosMasVendidos
                };

                return Ok(new { success = true, data = dashboard });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dashboard de ventas");
                return StatusCode(500, new { message = "Error al obtener dashboard", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene reporte de ventas por período con múltiples métricas
        /// </summary>
        [HttpGet("completo")]
        public async Task<ActionResult<object>> GetReporteCompleto(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] string agrupacion = "mes")
        {
            try
            {
                // Si no se proporcionan fechas, usar el mes actual
                fechaInicio ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                fechaFin ??= DateTime.Now;

                var reporte = await _ventaService.GenerarReporteVentasAsync(fechaInicio.Value, fechaFin.Value, agrupacion);
                var ventasPorEstado = await _ventaService.ObtenerVentasPorEstadoAsync(fechaInicio, fechaFin);
                var ventasPorMetodoPago = await _ventaService.ObtenerVentasPorMetodoPagoAsync(fechaInicio, fechaFin);

                var reporteCompleto = new
                {
                    resumen = new
                    {
                        reporte.FechaInicio,
                        reporte.FechaFin,
                        reporte.TotalVentas,
                        reporte.CantidadVentas,
                        reporte.PromedioVenta
                    },
                    ventasPorPeriodo = reporte.VentasPorPeriodo,
                    productosMasVendidos = reporte.ProductosMasVendidos,
                    clientesFrecuentes = reporte.ClientesFrecuentes,
                    ventasPorEstado,
                    ventasPorMetodoPago
                };

                return Ok(new { success = true, data = reporteCompleto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte completo de ventas");
                return StatusCode(500, new { message = "Error al generar reporte", error = ex.Message });
            }
        }

        /// <summary>
        /// Exporta reporte de ventas a CSV
        /// </summary>
        [HttpGet("exportar-csv")]
        public async Task<IActionResult> ExportarReporteCSV(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                fechaInicio ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                fechaFin ??= DateTime.Now;

                var ventas = await _ventaService.ObtenerVentasPaginadasAsync(
                    pageNumber: 1,
                    pageSize: int.MaxValue,
                    fechaInicio: fechaInicio,
                    fechaFin: fechaFin);

                var csv = GenerarCSV(ventas.Ventas);
                var fileName = $"reporte_ventas_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.csv";

                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar reporte CSV");
                return StatusCode(500, new { message = "Error al exportar reporte", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene comparativa de ventas por período
        /// </summary>
        [HttpGet("comparativa")]
        public async Task<ActionResult<object>> GetComparativaVentas(
            [FromQuery] DateTime fechaInicio1,
            [FromQuery] DateTime fechaFin1,
            [FromQuery] DateTime fechaInicio2,
            [FromQuery] DateTime fechaFin2)
        {
            try
            {
                var reporte1 = await _ventaService.GenerarReporteVentasAsync(fechaInicio1, fechaFin1);
                var reporte2 = await _ventaService.GenerarReporteVentasAsync(fechaInicio2, fechaFin2);

                var comparativa = new
                {
                    periodo1 = new
                    {
                        fechaInicio = fechaInicio1,
                        fechaFin = fechaFin1,
                        totalVentas = reporte1.TotalVentas,
                        cantidadVentas = reporte1.CantidadVentas,
                        promedioVenta = reporte1.PromedioVenta
                    },
                    periodo2 = new
                    {
                        fechaInicio = fechaInicio2,
                        fechaFin = fechaFin2,
                        totalVentas = reporte2.TotalVentas,
                        cantidadVentas = reporte2.CantidadVentas,
                        promedioVenta = reporte2.PromedioVenta
                    },
                    variaciones = new
                    {
                        ventasAbsolutas = reporte1.TotalVentas - reporte2.TotalVentas,
                        ventasPorcentaje = reporte2.TotalVentas != 0
                            ? Math.Round(((reporte1.TotalVentas - reporte2.TotalVentas) / reporte2.TotalVentas) * 100, 2)
                            : 0,
                        cantidadAbsoluta = reporte1.CantidadVentas - reporte2.CantidadVentas,
                        cantidadPorcentaje = reporte2.CantidadVentas != 0
                            ? Math.Round(((decimal)(reporte1.CantidadVentas - reporte2.CantidadVentas) / reporte2.CantidadVentas) * 100, 2)
                            : 0
                    }
                };

                return Ok(new { success = true, data = comparativa });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar comparativa de ventas");
                return StatusCode(500, new { message = "Error al generar comparativa", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene tendencias de ventas (últimos 12 meses)
        /// </summary>
        [HttpGet("tendencias")]
        public async Task<ActionResult<object>> GetTendenciasVentas()
        {
            try
            {
                var fechaFin = DateTime.Now;
                var fechaInicio = fechaFin.AddMonths(-12);

                var reporte = await _ventaService.GenerarReporteVentasAsync(fechaInicio, fechaFin, "mes");

                // Completar meses faltantes con ceros
                var tendencias = new List<object>();
                for (int i = 11; i >= 0; i--)
                {
                    var fecha = DateTime.Now.AddMonths(-i);
                    var periodo = $"{fecha.Year}-{fecha.Month:D2}";

                    var ventaMes = reporte.VentasPorPeriodo.FirstOrDefault(v => v.Periodo == periodo);

                    tendencias.Add(new
                    {
                        periodo = periodo,
                        periodoTexto = fecha.ToString("MMM yyyy"),
                        cantidadVentas = ventaMes?.CantidadVentas ?? 0,
                        montoTotal = ventaMes?.MontoTotal ?? 0,
                        promedioVenta = ventaMes?.PromedioVenta ?? 0
                    });
                }

                return Ok(new { success = true, data = tendencias });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tendencias de ventas");
                return StatusCode(500, new { message = "Error al obtener tendencias", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene métricas de rendimiento de ventas
        /// </summary>
        [HttpGet("metricas-rendimiento")]
        public async Task<ActionResult<object>> GetMetricasRendimiento(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                fechaInicio ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                fechaFin ??= DateTime.Now;

                var estadisticas = await _ventaService.ObtenerEstadisticasVentasAsync();
                var reporte = await _ventaService.GenerarReporteVentasAsync(fechaInicio.Value, fechaFin.Value);

                var diasPeriodo = (fechaFin.Value - fechaInicio.Value).Days + 1;
                var ventasPorDia = diasPeriodo > 0 ? (decimal)reporte.CantidadVentas / diasPeriodo : 0;
                var montoPorDia = diasPeriodo > 0 ? reporte.TotalVentas / diasPeriodo : 0;

                // Usar el nuevo método de cálculo de tasa de conversión
                var tasaConversion = await CalcularTasaConversionAsync(fechaInicio, fechaFin);

                var metricas = new
                {
                    ventasTotales = reporte.CantidadVentas,
                    montoTotal = reporte.TotalVentas,
                    promedioVenta = reporte.PromedioVenta,
                    ventasPorDia = Math.Round(ventasPorDia, 2),
                    montoPorDia = Math.Round(montoPorDia, 2),
                    tasaConversion = tasaConversion,
                    ventasPendientes = estadisticas.VentasPendientes,
                    ventasCompletadas = estadisticas.VentasCompletadas,
                    porcentajeCompletadas = estadisticas.TotalVentas > 0
                        ? Math.Round((decimal)estadisticas.VentasCompletadas / estadisticas.TotalVentas * 100, 2)
                        : 0,
                    eficienciaOperativa = await CalcularEficienciaOperativaAsync(fechaInicio.Value, fechaFin.Value)
                };

                return Ok(new { success = true, data = metricas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas de rendimiento");
                return StatusCode(500, new { message = "Error al obtener métricas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene métricas de conversión detalladas
        /// </summary>
        [HttpGet("metricas-conversion")]
        public async Task<ActionResult<object>> GetMetricasConversion(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                fechaInicio ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                fechaFin ??= DateTime.Now;

                var totalCotizaciones = await _context.Cotizaciones
                    .Where(c => c.FechaCotizacion >= fechaInicio && c.FechaCotizacion <= fechaFin)
                    .CountAsync();

                var cotizacionesAprobadas = await _context.Cotizaciones
                    .Where(c => c.FechaCotizacion >= fechaInicio &&
                               c.FechaCotizacion <= fechaFin &&
                               c.Estado == "Aprobada")
                    .CountAsync();

                var ventasDesdeCotizaciones = await _context.Ventas
                    .Where(v => v.CotizacionId != null &&
                               v.FechaVenta >= fechaInicio &&
                               v.FechaVenta <= fechaFin)
                    .CountAsync();

                var ventasDirectas = await _context.Ventas
                    .Where(v => v.CotizacionId == null &&
                               v.FechaVenta >= fechaInicio &&
                               v.FechaVenta <= fechaFin)
                    .CountAsync();

                var ventasCompletadas = await _context.Ventas
                    .Where(v => v.FechaVenta >= fechaInicio &&
                               v.FechaVenta <= fechaFin &&
                               v.EstadoVenta == "Entregado")
                    .CountAsync();

                var totalVentas = ventasDesdeCotizaciones + ventasDirectas;

                var metricas = new
                {
                    periodo = new { fechaInicio, fechaFin },
                    embudo = new
                    {
                        totalCotizaciones = totalCotizaciones,
                        cotizacionesAprobadas = cotizacionesAprobadas,
                        ventasDesdeCotizaciones = ventasDesdeCotizaciones,
                        ventasDirectas = ventasDirectas,
                        totalVentas = totalVentas,
                        ventasCompletadas = ventasCompletadas
                    },
                    tasasConversion = new
                    {
                        cotizacionesAAprobadas = totalCotizaciones > 0
                            ? Math.Round((decimal)cotizacionesAprobadas / totalCotizaciones * 100, 2)
                            : 0,
                        cotizacionesAVentas = totalCotizaciones > 0
                            ? Math.Round((decimal)ventasDesdeCotizaciones / totalCotizaciones * 100, 2)
                            : 0,
                        aprobadasAVentas = cotizacionesAprobadas > 0
                            ? Math.Round((decimal)ventasDesdeCotizaciones / cotizacionesAprobadas * 100, 2)
                            : 0,
                        ventasACompletadas = totalVentas > 0
                            ? Math.Round((decimal)ventasCompletadas / totalVentas * 100, 2)
                            : 0
                    },
                    rendimiento = new
                    {
                        tiempoCicloCotizacionVenta = await CalcularTiempoCicloPromedioAsync(fechaInicio.Value, fechaFin.Value),
                        valorPromedioVenta = await CalcularValorPromedioVentaAsync(fechaInicio.Value, fechaFin.Value),
                        eficienciaVentas = totalCotizaciones > 0
                            ? Math.Round((decimal)totalVentas / totalCotizaciones * 100, 2)
                            : 0
                    }
                };

                return Ok(new { success = true, data = metricas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas de conversión");
                return StatusCode(500, new { message = "Error al obtener métricas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene embudo de ventas detallado
        /// </summary>
        [HttpGet("embudo-ventas")]
        public async Task<ActionResult<object>> GetEmbudoVentas(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                fechaInicio ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                fechaFin ??= DateTime.Now;

                var cotizacionesSolicitadas = await _context.Cotizaciones
                    .Where(c => c.FechaCotizacion >= fechaInicio && c.FechaCotizacion <= fechaFin)
                    .CountAsync();

                var cotizacionesEnviadas = await _context.Cotizaciones
                    .Where(c => c.FechaCotizacion >= fechaInicio &&
                               c.FechaCotizacion <= fechaFin &&
                               (c.Estado == "Enviada" || c.Estado == "Aprobada" || c.Estado == "Vendida"))
                    .CountAsync();

                var cotizacionesAprobadas = await _context.Cotizaciones
                    .Where(c => c.FechaCotizacion >= fechaInicio &&
                               c.FechaCotizacion <= fechaFin &&
                               (c.Estado == "Aprobada" || c.Estado == "Vendida"))
                    .CountAsync();

                var ventasGeneradas = await _context.Ventas
                    .Where(v => v.FechaVenta >= fechaInicio && v.FechaVenta <= fechaFin)
                    .CountAsync();

                var ventasCompletadas = await _context.Ventas
                    .Where(v => v.FechaVenta >= fechaInicio &&
                               v.FechaVenta <= fechaFin &&
                               v.EstadoVenta == "Entregado")
                    .CountAsync();

                var embudo = new
                {
                    etapas = new[]
                    {
                        new {
                            nombre = "Cotizaciones Solicitadas",
                            cantidad = cotizacionesSolicitadas,
                            porcentaje = 100m,
                            color = "#e3f2fd"
                        },
                        new {
                            nombre = "Cotizaciones Enviadas",
                            cantidad = cotizacionesEnviadas,
                            porcentaje = cotizacionesSolicitadas > 0 ? Math.Round((decimal)cotizacionesEnviadas / cotizacionesSolicitadas * 100, 1) : 0,
                            color = "#bbdefb"
                        },
                        new {
                            nombre = "Cotizaciones Aprobadas",
                            cantidad = cotizacionesAprobadas,
                            porcentaje = cotizacionesSolicitadas > 0 ? Math.Round((decimal)cotizacionesAprobadas / cotizacionesSolicitadas * 100, 1) : 0,
                            color = "#90caf9"
                        },
                        new {
                            nombre = "Ventas Generadas",
                            cantidad = ventasGeneradas,
                            porcentaje = cotizacionesSolicitadas > 0 ? Math.Round((decimal)ventasGeneradas / cotizacionesSolicitadas * 100, 1) : 0,
                            color = "#64b5f6"
                        },
                        new {
                            nombre = "Ventas Completadas",
                            cantidad = ventasCompletadas,
                            porcentaje = cotizacionesSolicitadas > 0 ? Math.Round((decimal)ventasCompletadas / cotizacionesSolicitadas * 100, 1) : 0,
                            color = "#2196f3"
                        }
                    },
                    resumen = new
                    {
                        tasaConversionTotal = cotizacionesSolicitadas > 0 ? Math.Round((decimal)ventasCompletadas / cotizacionesSolicitadas * 100, 2) : 0,
                        eficienciaEnvio = cotizacionesSolicitadas > 0 ? Math.Round((decimal)cotizacionesEnviadas / cotizacionesSolicitadas * 100, 2) : 0,
                        tasaAprobacion = cotizacionesEnviadas > 0 ? Math.Round((decimal)cotizacionesAprobadas / cotizacionesEnviadas * 100, 2) : 0,
                        tasaCierre = cotizacionesAprobadas > 0 ? Math.Round((decimal)ventasGeneradas / cotizacionesAprobadas * 100, 2) : 0,
                        tasaCompletacion = ventasGeneradas > 0 ? Math.Round((decimal)ventasCompletadas / ventasGeneradas * 100, 2) : 0
                    }
                };

                return Ok(new { success = true, data = embudo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener embudo de ventas");
                return StatusCode(500, new { message = "Error al obtener embudo", error = ex.Message });
            }
        }

        #region Métodos privados

        private string GenerarCSV(List<VentaListDto> ventas)
        {
            var csv = new System.Text.StringBuilder();

            // Encabezados
            csv.AppendLine("Número Venta,Cliente,Email,Total,Fecha,Estado,Método Pago,Items");

            // Datos
            foreach (var venta in ventas)
            {
                csv.AppendLine($"{venta.NumeroVenta}," +
                              $"\"{venta.NombreCliente}\"," +
                              $"{venta.EmailCliente ?? ""}," +
                              $"{venta.Total}," +
                              $"{venta.FechaVenta:yyyy-MM-dd}," +
                              $"{venta.EstadoVenta}," +
                              $"{venta.MetodoPago ?? ""}," +
                              $"{venta.CantidadItems}");
            }

            return csv.ToString();
        }

        private async Task<decimal> CalcularTasaConversionAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                // Si no se especifican fechas, usar el mes actual
                fechaInicio ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                fechaFin ??= DateTime.Now;

                // Obtener cotizaciones en el período
                var totalCotizaciones = await _context.Cotizaciones
                    .Where(c => c.FechaCotizacion >= fechaInicio && c.FechaCotizacion <= fechaFin)
                    .CountAsync();

                // Obtener ventas creadas desde cotizaciones en el período
                var ventasDesdeCotizaciones = await _context.Ventas
                    .Where(v => v.CotizacionId != null &&
                               v.FechaVenta >= fechaInicio &&
                               v.FechaVenta <= fechaFin)
                    .CountAsync();

                // Calcular tasa de conversión de cotizaciones a ventas
                if (totalCotizaciones == 0)
                    return 0;

                return Math.Round((decimal)ventasDesdeCotizaciones / totalCotizaciones * 100, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular tasa de conversión");
                return 0;
            }
        }

        private async Task<decimal> CalcularEficienciaOperativaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var ventasEntregadas = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio &&
                           v.FechaVenta <= fechaFin &&
                           v.EstadoVenta == "Entregado")
                .CountAsync();

            var totalVentas = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio && v.FechaVenta <= fechaFin)
                .CountAsync();

            return totalVentas > 0 ? Math.Round((decimal)ventasEntregadas / totalVentas * 100, 2) : 0;
        }

        private async Task<decimal> CalcularTiempoCicloPromedioAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var ventasConCotizacion = await _context.Ventas
                .Where(v => v.CotizacionId != null &&
                           v.FechaVenta >= fechaInicio &&
                           v.FechaVenta <= fechaFin)
                .Include(v => v.Cotizacion)
                .ToListAsync();

            if (!ventasConCotizacion.Any())
                return 0;

            var tiemposCiclo = ventasConCotizacion
                .Where(v => v.Cotizacion != null)
                .Select(v => (v.FechaVenta - v.Cotizacion!.FechaCotizacion).TotalDays)
                .ToList();

            return Math.Round((decimal)tiemposCiclo.Average(), 1);
        }

        private async Task<decimal> CalcularValorPromedioVentaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio && v.FechaVenta <= fechaFin)
                .AverageAsync(v => (decimal?)v.Total) ?? 0;
        }

        #endregion
    }
}