using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAgro.API.Services;
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

        public ReporteVentasController(IVentaService ventaService, ILogger<ReporteVentasController> logger)
        {
            _ventaService = ventaService;
            _logger = logger;
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

                var metricas = new
                {
                    ventasTotales = reporte.CantidadVentas,
                    montoTotal = reporte.TotalVentas,
                    promedioVenta = reporte.PromedioVenta,
                    ventasPorDia = Math.Round(ventasPorDia, 2),
                    montoPorDia = Math.Round(montoPorDia, 2),
                    tasaConversion = CalcularTasaConversion(),
                    ventasPendientes = estadisticas.VentasPendientes,
                    ventasCompletadas = estadisticas.VentasCompletadas,
                    porcentajeCompletadas = estadisticas.TotalVentas > 0
                        ? Math.Round((decimal)estadisticas.VentasCompletadas / estadisticas.TotalVentas * 100, 2)
                        : 0
                };

                return Ok(new { success = true, data = metricas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas de rendimiento");
                return StatusCode(500, new { message = "Error al obtener métricas", error = ex.Message });
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

        private decimal CalcularTasaConversion()
        {
            // Aquí podrías implementar el cálculo de tasa de conversión
            // basado en cotizaciones vs ventas, o visitantes vs ventas, etc.
            // Por ahora retornamos un valor de ejemplo
            return 15.75m;
        }

        #endregion
    }
}