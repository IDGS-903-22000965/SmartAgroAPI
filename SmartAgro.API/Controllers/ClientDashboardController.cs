// SmartAgro.API/Controllers/ClientDashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.DTOs;
using System.Security.Claims;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/client/dashboard")]
    [Authorize(Roles = "Cliente")]
    public class ClientDashboardController : ControllerBase
    {
        private readonly SmartAgroDbContext _context;
        private readonly ILogger<ClientDashboardController> _logger;

        public ClientDashboardController(
            SmartAgroDbContext context,
            ILogger<ClientDashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el resumen completo del dashboard del cliente
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ClientDashboardDto>> GetClientDashboard()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var dashboard = new ClientDashboardDto
                {
                    ResumenCompras = await GetPurchaseSummaryAsync(currentUserId),
                    ProductosAdquiridos = await GetOwnedProductsSummaryAsync(currentUserId),
                    EstadoComentarios = await GetCommentsSummaryAsync(currentUserId),
                    ActividadReciente = await GetRecentActivityAsync(currentUserId),
                    EstadisticasGenerales = await GetGeneralStatsAsync(currentUserId),
                    NotificacionesImportantes = await GetImportantNotificationsAsync(currentUserId),
                    AccesosRapidos = GetQuickAccessItems()
                };

                return Ok(new { success = true, data = dashboard });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dashboard del cliente");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al cargar dashboard"
                });
            }
        }

        /// <summary>
        /// Obtiene notificaciones pendientes para el cliente
        /// </summary>
        [HttpGet("notifications")]
        public async Task<ActionResult<List<ClientNotificationDto>>> GetNotifications()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var notifications = await GetImportantNotificationsAsync(currentUserId);
                return Ok(new { success = true, data = notifications });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones del cliente");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener notificaciones"
                });
            }
        }

        /// <summary>
        /// Obtiene las estadísticas de actividad del cliente en los últimos meses
        /// </summary>
        [HttpGet("activity-chart")]
        public async Task<ActionResult<ClientActivityChartDto>> GetActivityChart()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var chartData = new ClientActivityChartDto();

                // Últimos 6 meses de actividad
                for (int i = 5; i >= 0; i--)
                {
                    var fecha = DateTime.Now.AddMonths(-i);
                    var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
                    var finMes = inicioMes.AddMonths(1).AddDays(-1);

                    var comprasEnMes = await _context.Ventas
                        .CountAsync(v => v.UsuarioId == currentUserId &&
                                    v.FechaVenta >= inicioMes &&
                                    v.FechaVenta <= finMes);

                    var gastoEnMes = await _context.Ventas
                        .Where(v => v.UsuarioId == currentUserId &&
                                   v.FechaVenta >= inicioMes &&
                                   v.FechaVenta <= finMes)
                        .SumAsync(v => (decimal?)v.Total) ?? 0;

                    var comentariosEnMes = await _context.Comentarios
                        .CountAsync(c => c.UsuarioId == currentUserId &&
                                    c.FechaComentario >= inicioMes &&
                                    c.FechaComentario <= finMes);

                    chartData.Meses.Add(fecha.ToString("MMM yyyy"));
                    chartData.Compras.Add(comprasEnMes);
                    chartData.Gastos.Add(gastoEnMes);
                    chartData.Comentarios.Add(comentariosEnMes);
                }

                return Ok(new { success = true, data = chartData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener gráfico de actividad");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener gráfico de actividad"
                });
            }
        }

        #region Métodos privados

        private async Task<ClientPurchaseSummaryDto> GetPurchaseSummaryAsync(string userId)
        {
            var ventas = await _context.Ventas
                .Where(v => v.UsuarioId == userId)
                .ToListAsync();

            var totalCompras = ventas.Count;
            var totalGastado = ventas.Sum(v => v.Total);
            var ultimaCompra = ventas.Any() ? ventas.Max(v => v.FechaVenta) : (DateTime?)null;

            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var comprasEsteMes = ventas.Count(v => v.FechaVenta >= inicioMes);
            var gastoEsteMes = ventas.Where(v => v.FechaVenta >= inicioMes).Sum(v => v.Total);

            return new ClientPurchaseSummaryDto
            {
                TotalCompras = totalCompras,
                TotalGastado = totalGastado,
                PromedioGasto = totalCompras > 0 ? totalGastado / totalCompras : 0,
                ComprasEsteMes = comprasEsteMes,
                GastoEsteMes = gastoEsteMes,
                UltimaCompra = ultimaCompra,
                VentasPendientes = ventas.Count(v => v.EstadoVenta == "Pendiente" || v.EstadoVenta == "Procesando"),
                VentasEntregadas = ventas.Count(v => v.EstadoVenta == "Entregado")
            };
        }

        private async Task<ClientOwnedProductsSummaryDto> GetOwnedProductsSummaryAsync(string userId)
        {
            var productosUnicos = await _context.DetallesVenta
                .Where(d => d.Venta.UsuarioId == userId)
                .Select(d => d.ProductoId)
                .Distinct()
                .CountAsync();

            var productoMasComprado = await _context.DetallesVenta
                .Where(d => d.Venta.UsuarioId == userId)
                .GroupBy(d => new { d.ProductoId, d.Producto.Nombre })
                .OrderByDescending(g => g.Sum(d => d.Cantidad))
                .Select(g => g.Key.Nombre)
                .FirstOrDefaultAsync();

            var productosConGarantiaVigente = await CalcularProductosConGarantiaAsync(userId);

            return new ClientOwnedProductsSummaryDto
            {
                TotalProductosUnicos = productosUnicos,
                ProductoMasComprado = productoMasComprado ?? "Ninguno",
                ProductosConGarantiaVigente = productosConGarantiaVigente
            };
        }

        private async Task<ClientCommentsSummaryDto> GetCommentsSummaryAsync(string userId)
        {
            var totalComentarios = await _context.Comentarios
                .CountAsync(c => c.UsuarioId == userId);

            var comentariosAprobados = await _context.Comentarios
                .CountAsync(c => c.UsuarioId == userId && c.Aprobado);

            var comentariosPendientes = await _context.Comentarios
                .CountAsync(c => c.UsuarioId == userId && !c.Aprobado && c.Activo);

            var calificacionPromedio = await _context.Comentarios
                .Where(c => c.UsuarioId == userId)
                .AverageAsync(c => (double?)c.Calificacion) ?? 0;

            return new ClientCommentsSummaryDto
            {
                TotalComentarios = totalComentarios,
                ComentariosAprobados = comentariosAprobados,
                ComentariosPendientes = comentariosPendientes,
                CalificacionPromedio = Math.Round(calificacionPromedio, 1)
            };
        }

        private async Task<List<ClientActivityItemDto>> GetRecentActivityAsync(string userId)
        {
            var activities = new List<ClientActivityItemDto>();

            // Últimas compras
            var recentPurchases = await _context.Ventas
                .Where(v => v.UsuarioId == userId)
                .OrderByDescending(v => v.FechaVenta)
                .Take(3)
                .Select(v => new ClientActivityItemDto
                {
                    Tipo = "Compra",
                    Descripcion = $"Compra #{v.NumeroVenta} por ${v.Total:F2}",
                    Fecha = v.FechaVenta,
                    Icono = "🛒",
                    Estado = v.EstadoVenta
                })
                .ToListAsync();

            activities.AddRange(recentPurchases);

            // Últimos comentarios
            var recentComments = await _context.Comentarios
                .Where(c => c.UsuarioId == userId)
                .Include(c => c.Producto)
                .OrderByDescending(c => c.FechaComentario)
                .Take(3)
                .Select(c => new ClientActivityItemDto
                {
                    Tipo = "Comentario",
                    Descripcion = $"Comentario en {c.Producto.Nombre}",
                    Fecha = c.FechaComentario,
                    Icono = "💬",
                    Estado = c.Aprobado ? "Aprobado" : "Pendiente"
                })
                .ToListAsync();

            activities.AddRange(recentComments);

            return activities.OrderByDescending(a => a.Fecha).Take(5).ToList();
        }

        private async Task<ClientGeneralStatsDto> GetGeneralStatsAsync(string userId)
        {
            var fechaRegistro = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.FechaRegistro)
                .FirstOrDefaultAsync();

            var diasComoCliente = (DateTime.Now - fechaRegistro).Days;

            var productosFavoritos = await _context.DetallesVenta
                .Where(d => d.Venta.UsuarioId == userId)
                .GroupBy(d => d.Producto.Nombre)
                .OrderByDescending(g => g.Sum(d => d.Cantidad))
                .Take(3)
                .Select(g => g.Key)
                .ToListAsync();

            return new ClientGeneralStatsDto
            {
                DiasComoCliente = diasComoCliente,
                FechaRegistro = fechaRegistro,
                ProductosFavoritos = productosFavoritos
            };
        }

        private async Task<List<ClientNotificationDto>> GetImportantNotificationsAsync(string userId)
        {
            var notifications = new List<ClientNotificationDto>();

            // Verificar garantías próximas a vencer
            var ventasConGarantia = await _context.Ventas
                .Where(v => v.UsuarioId == userId)
                .OrderBy(v => v.FechaVenta)
                .ToListAsync();

            foreach (var venta in ventasConGarantia)
            {
                var fechaVencimiento = venta.FechaVenta.AddMonths(24); // 24 meses de garantía
                var diasRestantes = (fechaVencimiento - DateTime.Now).Days;

                if (diasRestantes > 0 && diasRestantes <= 30)
                {
                    notifications.Add(new ClientNotificationDto
                    {
                        Tipo = "Garantía",
                        Titulo = "Garantía próxima a vencer",
                        Mensaje = $"La garantía de tu compra #{venta.NumeroVenta} vence en {diasRestantes} días",
                        Fecha = DateTime.Now,
                        Prioridad = "Media",
                        Icono = "⚠️"
                    });
                }
            }

            // Verificar productos que pueden ser comentados
            var productosParaComentar = await _context.DetallesVenta
                .Where(d => d.Venta.UsuarioId == userId)
                .Where(d => !_context.Comentarios.Any(c =>
                    c.UsuarioId == userId &&
                    c.ProductoId == d.ProductoId))
                .GroupBy(d => d.Producto.Nombre)
                .CountAsync();

            if (productosParaComentar > 0)
            {
                notifications.Add(new ClientNotificationDto
                {
                    Tipo = "Comentario",
                    Titulo = "Productos sin comentar",
                    Mensaje = $"Tienes {productosParaComentar} producto(s) que puedes comentar y calificar",
                    Fecha = DateTime.Now,
                    Prioridad = "Baja",
                    Icono = "⭐"
                });
            }

            // Verificar si hay nuevos recursos disponibles (simulado)
            var ultimaCompra = await _context.Ventas
                .Where(v => v.UsuarioId == userId)
                .OrderByDescending(v => v.FechaVenta)
                .FirstOrDefaultAsync();

            if (ultimaCompra != null && ultimaCompra.FechaVenta >= DateTime.Now.AddDays(-7))
            {
                notifications.Add(new ClientNotificationDto
                {
                    Tipo = "Recursos",
                    Titulo = "Nuevos recursos disponibles",
                    Mensaje = "Ya están disponibles los manuales y recursos para tu compra reciente",
                    Fecha = DateTime.Now,
                    Prioridad = "Media",
                    Icono = "📚"
                });
            }

            return notifications.OrderByDescending(n => n.Fecha).ToList();
        }

        private async Task<int> CalcularProductosConGarantiaAsync(string userId)
        {
            var ventasConGarantia = await _context.DetallesVenta
                .Where(d => d.Venta.UsuarioId == userId)
                .GroupBy(d => d.ProductoId)
                .Select(g => g.Min(d => d.Venta.FechaVenta))
                .ToListAsync();

            var productosConGarantiaVigente = 0;
            foreach (var fechaCompra in ventasConGarantia)
            {
                var fechaVencimiento = fechaCompra.AddMonths(24); // 24 meses de garantía
                if (fechaVencimiento > DateTime.Now)
                {
                    productosConGarantiaVigente++;
                }
            }

            return productosConGarantiaVigente;
        }

        // Actualizar el método GetQuickAccessItems() en ClientDashboardController.cs
        private List<ClientQuickAccessDto> GetQuickAccessItems()
        {
            return new List<ClientQuickAccessDto>
    {
        new ClientQuickAccessDto
        {
            Titulo = "Mis Compras",
            Descripcion = "Ver historial de compras",
            Icono = "🛒",
            Url = "/cliente/mis-compras", // ✅ Corregido
            Color = "#4CAF50"
        },
        new ClientQuickAccessDto
        {
            Titulo = "Mis Productos",
            Descripcion = "Productos adquiridos y recursos",
            Icono = "📦",
            Url = "/cliente/mis-compras", // ✅ Usar mis-compras (tab productos)
            Color = "#2196F3"
        },
        new ClientQuickAccessDto
        {
            Titulo = "Comentarios",
            Descripcion = "Mis comentarios y calificaciones",
            Icono = "⭐",
            Url = "/cliente/comentarios", // ✅ Corregido
            Color = "#FF9800"
        },
        new ClientQuickAccessDto
        {
            Titulo = "Perfil",
            Descripcion = "Actualizar información personal",
            Icono = "👤",
            Url = "/cliente/perfil", // ✅ Corregido
            Color = "#9C27B0"
        },
        new ClientQuickAccessDto
        {
            Titulo = "Soporte",
            Descripcion = "Contactar soporte técnico",
            Icono = "🛠️",
            Url = "/contacto", // ✅ Usar página de contacto existente
            Color = "#F44336"
        },
        new ClientQuickAccessDto
        {
            Titulo = "Recursos",
            Descripcion = "Manuales y documentación",
            Icono = "📚",
            Url = "/cliente/documentacion", // ✅ Corregido
            Color = "#607D8B"
        }
    };
        }

        #endregion
    }
}

// DTOs para el dashboard del cliente
namespace SmartAgro.Models.DTOs
{
    public class ClientDashboardDto
    {
        public ClientPurchaseSummaryDto ResumenCompras { get; set; } = new ClientPurchaseSummaryDto();
        public ClientOwnedProductsSummaryDto ProductosAdquiridos { get; set; } = new ClientOwnedProductsSummaryDto();
        public ClientCommentsSummaryDto EstadoComentarios { get; set; } = new ClientCommentsSummaryDto();
        public List<ClientActivityItemDto> ActividadReciente { get; set; } = new List<ClientActivityItemDto>();
        public ClientGeneralStatsDto EstadisticasGenerales { get; set; } = new ClientGeneralStatsDto();
        public List<ClientNotificationDto> NotificacionesImportantes { get; set; } = new List<ClientNotificationDto>();
        public List<ClientQuickAccessDto> AccesosRapidos { get; set; } = new List<ClientQuickAccessDto>();
    }

    public class ClientPurchaseSummaryDto
    {
        public int TotalCompras { get; set; }
        public decimal TotalGastado { get; set; }
        public decimal PromedioGasto { get; set; }
        public int ComprasEsteMes { get; set; }
        public decimal GastoEsteMes { get; set; }
        public DateTime? UltimaCompra { get; set; }
        public int VentasPendientes { get; set; }
        public int VentasEntregadas { get; set; }
    }

    public class ClientOwnedProductsSummaryDto
    {
        public int TotalProductosUnicos { get; set; }
        public string ProductoMasComprado { get; set; } = string.Empty;
        public int ProductosConGarantiaVigente { get; set; }
    }

    public class ClientCommentsSummaryDto
    {
        public int TotalComentarios { get; set; }
        public int ComentariosAprobados { get; set; }
        public int ComentariosPendientes { get; set; }
        public double CalificacionPromedio { get; set; }
    }

    public class ClientActivityItemDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Icono { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class ClientGeneralStatsDto
    {
        public int DiasComoCliente { get; set; }
        public DateTime FechaRegistro { get; set; }
        public List<string> ProductosFavoritos { get; set; } = new List<string>();
    }

    public class ClientNotificationDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Prioridad { get; set; } = string.Empty; // Alta, Media, Baja
        public string Icono { get; set; } = string.Empty;
    }

    public class ClientQuickAccessDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class ClientActivityChartDto
    {
        public List<string> Meses { get; set; } = new List<string>();
        public List<int> Compras { get; set; } = new List<int>();
        public List<decimal> Gastos { get; set; } = new List<decimal>();
        public List<int> Comentarios { get; set; } = new List<int>();
    }
}