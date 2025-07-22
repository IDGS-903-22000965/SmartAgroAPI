using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using System.Security.Claims;

namespace SmartAgro.API.Controllers
{
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
                var fechaInicio = DateTime.Now.AddMonths(-12);

                var metricas = new
                {
                    TotalCotizaciones = await _context.Cotizaciones.CountAsync(),
                    CotizacionesPendientes = await _context.Cotizaciones.CountAsync(c => c.Estado == "Pendiente"),
                    CotizacionesAprobadas = await _context.Cotizaciones.CountAsync(c => c.Estado == "Aprobada"),
                    TotalVentas = await _context.Ventas.CountAsync(),
                    VentasDelMes = await _context.Ventas.CountAsync(v => v.FechaVenta.Month == DateTime.Now.Month),
                    TotalClientes = await _context.Users.CountAsync(u => u.Activo),
                    ClientesNuevosDelMes = await _context.Users.CountAsync(u => u.FechaRegistro.Month == DateTime.Now.Month),
                    IngresosTotales = await _context.Ventas.SumAsync(v => (decimal?)v.Total) ?? 0m,
                    IngresosDelMes = await _context.Ventas
                        .Where(v => v.FechaVenta.Month == DateTime.Now.Month)
                        .SumAsync(v => (decimal?)v.Total) ?? 0m,
                    PromedioCalificacion = await _context.Comentarios
                        .Where(c => c.Aprobado && c.Activo)
                        .AverageAsync(c => (double?)c.Calificacion) ?? 0,
                    TotalComentarios = await _context.Comentarios.CountAsync(c => c.Activo),
                    ComentariosPendientes = await _context.Comentarios.CountAsync(c => !c.Aprobado && c.Activo)
                };

                // Datos para gráficos
                var ventasPorMes = await _context.Ventas
                    .Where(v => v.FechaVenta >= fechaInicio)
                    .GroupBy(v => new { v.FechaVenta.Year, v.FechaVenta.Month })
                    .Select(g => new {
                        Mes = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Cantidad = g.Count(),
                        Total = g.Sum(v => v.Total)
                    })
                    .OrderBy(x => x.Mes)
                    .ToListAsync();

                var cotizacionesPorEstado = await _context.Cotizaciones
                    .GroupBy(c => c.Estado)
                    .Select(g => new {
                        Estado = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        metricas,
                        ventasPorMes,
                        cotizacionesPorEstado
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener las métricas",
                    error = ex.Message
                });
            }
        }

        [HttpGet("actividad-reciente")]
        public async Task<IActionResult> ObtenerActividadReciente()
        {
            try
            {
                var fecha = DateTime.Now.AddDays(-7);

                var actividades = new List<object>();

                // Cotizaciones recientes
                var cotizacionesRecientes = await _context.Cotizaciones
                    .Where(c => c.FechaCotizacion >= fecha)
                    .OrderByDescending(c => c.FechaCotizacion)
                    .Take(5)
                    .Select(c => new {
                        Tipo = "Cotización",
                        Descripcion = $"Nueva cotización de {c.NombreCliente}",
                        Fecha = c.FechaCotizacion,
                        Estado = c.Estado
                    })
                    .ToListAsync();

                // Ventas recientes
                var ventasRecientes = await _context.Ventas
                    .Where(v => v.FechaVenta >= fecha)
                    .OrderByDescending(v => v.FechaVenta)
                    .Take(5)
                    .Select(v => new {
                        Tipo = "Venta",
                        Descripcion = $"Venta #{v.NumeroVenta} por ${v.Total:F2}",
                        Fecha = v.FechaVenta,
                        Estado = v.EstadoVenta
                    })
                    .ToListAsync();

                // Comentarios recientes
                var comentariosRecientes = await _context.Comentarios
                    .Where(c => c.FechaComentario >= fecha)
                    .OrderByDescending(c => c.FechaComentario)
                    .Take(5)
                    .Include(c => c.Usuario)
                    .Select(c => new {
                        Tipo = "Comentario",
                        Descripcion = $"Nuevo comentario de {c.Usuario.Nombre} ({c.Calificacion} estrellas)",
                        Fecha = c.FechaComentario,
                        Estado = c.Aprobado ? "Aprobado" : "Pendiente"
                    })
                    .ToListAsync();

                actividades.AddRange(cotizacionesRecientes);
                actividades.AddRange(ventasRecientes);
                actividades.AddRange(comentariosRecientes);

                var actividadesOrdenadas = actividades
                    .OrderByDescending(a => ((dynamic)a).Fecha)
                    .Take(10)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = actividadesOrdenadas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener la actividad reciente",
                    error = ex.Message
                });
            }
        }
    }
}