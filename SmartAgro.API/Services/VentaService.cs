using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.DTOs;
using SmartAgro.Models.DTOs.Ventas;
using SmartAgro.Models.Entities;
using System.Globalization;

namespace SmartAgro.API.Services
{
    public class VentaService : IVentaService
    {
        private readonly SmartAgroDbContext _context;
        private readonly ILogger<VentaService> _logger;
        private readonly ICosteoFifoService _costeoFifoService;

        public VentaService(
           SmartAgroDbContext context,
           ILogger<VentaService> logger,
           ICosteoFifoService costeoFifoService)
        {
            _context = context;
            _logger = logger;
            _costeoFifoService = costeoFifoService;
        }

        #region Métodos CRUD básicos

        public async Task<PaginatedVentasDto> ObtenerVentasPaginadasAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? estado = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            string? metodoPago = null)
        {
            try
            {
                var query = _context.Ventas
                    .Include(v => v.Usuario)
                    .Include(v => v.Cotizacion)
                    .Include(v => v.Detalles)
                    .AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(v =>
                        v.NumeroVenta.Contains(searchTerm) ||
                        v.NombreCliente.Contains(searchTerm) ||
                        (v.EmailCliente != null && v.EmailCliente.Contains(searchTerm)));
                }

                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(v => v.EstadoVenta == estado);
                }

                if (fechaInicio.HasValue)
                {
                    query = query.Where(v => v.FechaVenta >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(v => v.FechaVenta <= fechaFin.Value);
                }

                if (!string.IsNullOrEmpty(metodoPago))
                {
                    query = query.Where(v => v.MetodoPago == metodoPago);
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var ventas = await query
                    .OrderByDescending(v => v.FechaVenta)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(v => new VentaListDto
                    {
                        Id = v.Id,
                        NumeroVenta = v.NumeroVenta,
                        NombreCliente = v.NombreCliente,
                        EmailCliente = v.EmailCliente,
                        Total = v.Total,
                        FechaVenta = v.FechaVenta,
                        EstadoVenta = v.EstadoVenta,
                        MetodoPago = v.MetodoPago,
                        CantidadItems = v.Detalles.Count(),
                        NumeroCotizacion = v.Cotizacion != null ? v.Cotizacion.NumeroCotizacion : null
                    })
                    .ToListAsync();

                return new PaginatedVentasDto
                {
                    Ventas = ventas,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = pageNumber < totalPages,
                    HasPreviousPage = pageNumber > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas paginadas");
                throw new Exception($"Error al obtener ventas: {ex.Message}");
            }
        }

        public async Task<VentaDetalleDto?> ObtenerVentaDetalleAsync(int id)
        {
            try
            {
                var venta = await _context.Ventas
                    .Include(v => v.Usuario)
                    .Include(v => v.Cotizacion)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (venta == null) return null;

                return new VentaDetalleDto
                {
                    Id = venta.Id,
                    NumeroVenta = venta.NumeroVenta,
                    UsuarioId = venta.UsuarioId,
                    NombreUsuario = $"{venta.Usuario.Nombre} {venta.Usuario.Apellidos}",
                    CotizacionId = venta.CotizacionId,
                    NumeroCotizacion = venta.Cotizacion?.NumeroCotizacion,
                    NombreCliente = venta.NombreCliente,
                    EmailCliente = venta.EmailCliente,
                    TelefonoCliente = venta.TelefonoCliente,
                    DireccionEntrega = venta.DireccionEntrega,
                    Subtotal = venta.Subtotal,
                    Impuestos = venta.Impuestos,
                    Total = venta.Total,
                    FechaVenta = venta.FechaVenta,
                    EstadoVenta = venta.EstadoVenta,
                    MetodoPago = venta.MetodoPago,
                    Observaciones = venta.Observaciones,
                    Detalles = venta.Detalles.Select(d => new DetalleVentaDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        NombreProducto = d.Producto.Nombre,
                        DescripcionProducto = d.Producto.Descripcion,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal,
                        ImagenProducto = d.Producto.ImagenPrincipal
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de venta {VentaId}", id);
                throw new Exception($"Error al obtener venta: {ex.Message}");
            }
        }

        public async Task<List<VentaListDto>> ObtenerVentasPorUsuarioAsync(string usuarioId)
        {
            try
            {
                return await _context.Ventas
                    .Where(v => v.UsuarioId == usuarioId)
                    .Include(v => v.Cotizacion)
                    .Include(v => v.Detalles)
                    .OrderByDescending(v => v.FechaVenta)
                    .Select(v => new VentaListDto
                    {
                        Id = v.Id,
                        NumeroVenta = v.NumeroVenta,
                        NombreCliente = v.NombreCliente,
                        EmailCliente = v.EmailCliente,
                        Total = v.Total,
                        FechaVenta = v.FechaVenta,
                        EstadoVenta = v.EstadoVenta,
                        MetodoPago = v.MetodoPago,
                        CantidadItems = v.Detalles.Count(),
                        NumeroCotizacion = v.Cotizacion != null ? v.Cotizacion.NumeroCotizacion : null
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas del usuario {UsuarioId}", usuarioId);
                throw new Exception($"Error al obtener ventas del usuario: {ex.Message}");
            }
        }

        public async Task<ServiceResult> CrearVentaAsync(CreateVentaDto createVentaDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var numeroVenta = await GenerarNumeroVentaAsync();

                decimal subtotal = 0;
                var detallesVenta = new List<DetalleVenta>();

                foreach (var detalleDto in createVentaDto.Detalles)
                {
                    var producto = await _context.Productos
                        .Include(p => p.ProductoMateriasPrimas)
                        .FirstAsync(p => p.Id == detalleDto.ProductoId);

                    if (!producto.Activo)
                        return ServiceResult.ErrorResult($"Producto ID {detalleDto.ProductoId} está inactivo");

                    var subtotalDetalle = detalleDto.Cantidad * detalleDto.PrecioUnitario;
                    subtotal += subtotalDetalle;

                    detallesVenta.Add(new DetalleVenta
                    {
                        ProductoId = detalleDto.ProductoId,
                        Cantidad = detalleDto.Cantidad,
                        PrecioUnitario = detalleDto.PrecioUnitario,
                        Subtotal = subtotalDetalle
                    });

                    // ✅ Registrar salidas FIFO por cada material
                    foreach (var material in producto.ProductoMateriasPrimas)
                    {
                        var cantidadNecesaria = material.CantidadRequerida * detalleDto.Cantidad;

                        try
                        {
                            var costoMaterial = await _costeoFifoService.ObtenerCostoSalidaFifoAsync(
                                material.MateriaPrimaId,
                                cantidadNecesaria);

                            var movimiento = new MovimientoStock
                            {
                                MateriaPrimaId = material.MateriaPrimaId,
                                Tipo = "Salida",
                                Cantidad = cantidadNecesaria,
                                CostoUnitario = cantidadNecesaria > 0 ? costoMaterial / cantidadNecesaria : 0,
                                Referencia = numeroVenta,
                                Observaciones = $"Venta {numeroVenta} - Producto {producto.Nombre}",
                                Fecha = DateTime.Now
                            };

                            _context.MovimientosStock.Add(movimiento);

                            // Actualizar stock de materia prima
                            var materiaPrima = await _context.MateriasPrimas.FindAsync(material.MateriaPrimaId);
                            if (materiaPrima != null)
                            {
                                materiaPrima.Stock = Math.Max(0, materiaPrima.Stock - (int)cantidadNecesaria);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error en costeo FIFO para material {MaterialId}, usando costo unitario actual", material.MateriaPrimaId);

                            // Fallback: usar costo unitario actual
                            var movimiento = new MovimientoStock
                            {
                                MateriaPrimaId = material.MateriaPrimaId,
                                Tipo = "Salida",
                                Cantidad = cantidadNecesaria,
                                CostoUnitario = material.CostoUnitario,
                                Referencia = numeroVenta,
                                Observaciones = $"Venta {numeroVenta} - Producto {producto.Nombre} (Fallback)",
                                Fecha = DateTime.Now
                            };

                            _context.MovimientosStock.Add(movimiento);

                            // Actualizar stock de materia prima
                            var materiaPrima = await _context.MateriasPrimas.FindAsync(material.MateriaPrimaId);
                            if (materiaPrima != null)
                            {
                                materiaPrima.Stock = Math.Max(0, materiaPrima.Stock - (int)cantidadNecesaria);
                            }
                        }
                    }
                }

                var impuestos = subtotal * 0.16m;
                var total = subtotal + impuestos;

                var venta = new Venta
                {
                    NumeroVenta = numeroVenta,
                    UsuarioId = createVentaDto.UsuarioId,
                    NombreCliente = createVentaDto.NombreCliente,
                    EmailCliente = createVentaDto.EmailCliente,
                    TelefonoCliente = createVentaDto.TelefonoCliente,
                    DireccionEntrega = createVentaDto.DireccionEntrega,
                    Subtotal = subtotal,
                    Impuestos = impuestos,
                    Total = total,
                    EstadoVenta = "Pendiente",
                    MetodoPago = createVentaDto.MetodoPago,
                    Observaciones = createVentaDto.Observaciones,
                    Detalles = detallesVenta
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Venta {NumeroVenta} creada exitosamente", numeroVenta);
                return ServiceResult.SuccessResult($"Venta {numeroVenta} creada exitosamente", new { Id = venta.Id, NumeroVenta = numeroVenta });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear venta");
                return ServiceResult.ErrorResult($"Error al crear venta: {ex.Message}");
            }
        }
        public async Task<ServiceResult> CrearVentaDesdeCotizacionAsync(int cotizacionId, CreateVentaFromCotizacionDto ventaDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cotizacion = await _context.Cotizaciones
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c => c.Id == cotizacionId);

                if (cotizacion == null)
                {
                    return ServiceResult.ErrorResult("Cotización no encontrada");
                }

                if (cotizacion.Estado != "Aprobada")
                {
                    return ServiceResult.ErrorResult("Solo se pueden crear ventas desde cotizaciones aprobadas");
                }

                // Verificar si ya existe una venta para esta cotización
                var ventaExistente = await _context.Ventas.FirstOrDefaultAsync(v => v.CotizacionId == cotizacionId);
                if (ventaExistente != null)
                {
                    return ServiceResult.ErrorResult("Ya existe una venta para esta cotización");
                }

                var numeroVenta = await GenerarNumeroVentaAsync();

                // Crear venta basada en la cotización
                var venta = new Venta
                {
                    NumeroVenta = numeroVenta,
                    UsuarioId = cotizacion.UsuarioId ?? string.Empty,
                    CotizacionId = cotizacionId,
                    NombreCliente = cotizacion.NombreCliente,
                    EmailCliente = cotizacion.EmailCliente,
                    TelefonoCliente = cotizacion.TelefonoCliente,
                    DireccionEntrega = ventaDto.DireccionEntrega ?? cotizacion.DireccionInstalacion,
                    Subtotal = cotizacion.Subtotal,
                    Impuestos = cotizacion.Impuestos,
                    Total = cotizacion.Total,
                    EstadoVenta = "Pendiente",
                    MetodoPago = ventaDto.MetodoPago,
                    Observaciones = ventaDto.Observaciones
                };

                // Crear detalles de venta basados en la cotización
                foreach (var detalleCotizacion in cotizacion.Detalles)
                {
                    venta.Detalles.Add(new DetalleVenta
                    {
                        ProductoId = detalleCotizacion.ProductoId,
                        Cantidad = detalleCotizacion.Cantidad,
                        PrecioUnitario = detalleCotizacion.PrecioUnitario,
                        Subtotal = detalleCotizacion.Subtotal
                    });
                }

                _context.Ventas.Add(venta);

                // Actualizar estado de la cotización
                cotizacion.Estado = "Vendida";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Venta {NumeroVenta} creada desde cotización {NumeroCotizacion}", numeroVenta, cotizacion.NumeroCotizacion);
                return ServiceResult.SuccessResult($"Venta {numeroVenta} creada exitosamente desde cotización", new { Id = venta.Id, NumeroVenta = numeroVenta });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear venta desde cotización {CotizacionId}", cotizacionId);
                return ServiceResult.ErrorResult($"Error al crear venta: {ex.Message}");
            }
        }

        public async Task<ServiceResult> ActualizarEstadoVentaAsync(int id, ActualizarEstadoVentaDto estadoDto)
        {
            try
            {
                var venta = await _context.Ventas.FindAsync(id);
                if (venta == null)
                {
                    return ServiceResult.ErrorResult("Venta no encontrada");
                }

                var estadosValidos = new[] { "Pendiente", "Procesando", "Enviado", "Entregado", "Cancelado" };
                if (!estadosValidos.Contains(estadoDto.EstadoVenta))
                {
                    return ServiceResult.ErrorResult("Estado no válido");
                }

                var estadoAnterior = venta.EstadoVenta;
                venta.EstadoVenta = estadoDto.EstadoVenta;

                if (!string.IsNullOrEmpty(estadoDto.Observaciones))
                {
                    venta.Observaciones = string.IsNullOrEmpty(venta.Observaciones)
                        ? estadoDto.Observaciones
                        : $"{venta.Observaciones}\n{DateTime.Now:dd/MM/yyyy HH:mm}: {estadoDto.Observaciones}";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Estado de venta {NumeroVenta} cambiado de {EstadoAnterior} a {EstadoNuevo}",
                    venta.NumeroVenta, estadoAnterior, estadoDto.EstadoVenta);

                return ServiceResult.SuccessResult($"Estado actualizado a {estadoDto.EstadoVenta}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de venta {VentaId}", id);
                return ServiceResult.ErrorResult($"Error al actualizar estado: {ex.Message}");
            }
        }

        #endregion

        #region Estadísticas y Reportes

        public async Task<VentaStatsDto> ObtenerEstadisticasVentasAsync()
        {
            try
            {
                var hoy = DateTime.Today;
                var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
                var inicioMesAnterior = inicioMes.AddMonths(-1);
                var finMesAnterior = inicioMes.AddDays(-1);

                var totalVentas = await _context.Ventas.CountAsync();
                var montoTotalVentas = await _context.Ventas.SumAsync(v => (decimal?)v.Total) ?? 0;

                var ventasHoy = await _context.Ventas.CountAsync(v => v.FechaVenta.Date == hoy);
                var montoVentasHoy = await _context.Ventas
                    .Where(v => v.FechaVenta.Date == hoy)
                    .SumAsync(v => (decimal?)v.Total) ?? 0;

                var ventasEsteMes = await _context.Ventas.CountAsync(v => v.FechaVenta >= inicioMes);
                var montoVentasEsteMes = await _context.Ventas
                    .Where(v => v.FechaVenta >= inicioMes)
                    .SumAsync(v => (decimal?)v.Total) ?? 0;

                var montoMesAnterior = await _context.Ventas
                    .Where(v => v.FechaVenta >= inicioMesAnterior && v.FechaVenta <= finMesAnterior)
                    .SumAsync(v => (decimal?)v.Total) ?? 0;

                var ventasPendientes = await _context.Ventas.CountAsync(v => v.EstadoVenta == "Pendiente");
                var ventasCompletadas = await _context.Ventas.CountAsync(v => v.EstadoVenta == "Entregado");

                var promedioVentaDiaria = totalVentas > 0 ? montoTotalVentas / (decimal)(DateTime.Now - DateTime.Now.AddMonths(-1)).TotalDays : 0;
                var crecimientoMesAnterior = montoMesAnterior > 0 ? ((montoVentasEsteMes - montoMesAnterior) / montoMesAnterior) * 100 : 0;

                // Distribución por estado
                var ventasPorEstado = await _context.Ventas
                    .GroupBy(v => v.EstadoVenta)
                    .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                    .ToDictionaryAsync(x => x.Estado, x => x.Cantidad);

                // Distribución por método de pago
                var ventasPorMetodoPago = await _context.Ventas
                    .Where(v => !string.IsNullOrEmpty(v.MetodoPago))
                    .GroupBy(v => v.MetodoPago!)
                    .Select(g => new { Metodo = g.Key, Cantidad = g.Count() })
                    .ToDictionaryAsync(x => x.Metodo, x => x.Cantidad);

                return new VentaStatsDto
                {
                    TotalVentas = totalVentas,
                    MontoTotalVentas = montoTotalVentas,
                    VentasHoy = ventasHoy,
                    MontoVentasHoy = montoVentasHoy,
                    VentasEsteMes = ventasEsteMes,
                    MontoVentasEsteMes = montoVentasEsteMes,
                    VentasPendientes = ventasPendientes,
                    VentasCompletadas = ventasCompletadas,
                    PromedioVentaDiaria = Math.Round(promedioVentaDiaria, 2),
                    CrecimientoMesAnterior = Math.Round(crecimientoMesAnterior, 2),
                    VentasPorEstado = ventasPorEstado,
                    VentasPorMetodoPago = ventasPorMetodoPago
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de ventas");
                throw new Exception($"Error al obtener estadísticas: {ex.Message}");
            }
        }

        public async Task<ReporteVentasDto> GenerarReporteVentasAsync(DateTime fechaInicio, DateTime fechaFin, string? agrupacion = "mes")
        {
            try
            {
                var ventas = await _context.Ventas
                    .Where(v => v.FechaVenta >= fechaInicio && v.FechaVenta <= fechaFin)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .ToListAsync();

                var totalVentas = ventas.Sum(v => v.Total);
                var cantidadVentas = ventas.Count;
                var promedioVenta = cantidadVentas > 0 ? totalVentas / cantidadVentas : 0;

                // Agrupar ventas por período
                var ventasPorPeriodo = new List<VentaPorPeriodoDto>();

                switch (agrupacion?.ToLower())
                {
                    case "dia":
                        ventasPorPeriodo = ventas
                            .GroupBy(v => v.FechaVenta.Date)
                            .Select(g => new VentaPorPeriodoDto
                            {
                                Periodo = g.Key.ToString("yyyy-MM-dd"),
                                PeriodoTexto = g.Key.ToString("dd 'de' MMMM yyyy", new CultureInfo("es-ES")),
                                CantidadVentas = g.Count(),
                                MontoTotal = g.Sum(v => v.Total),
                                PromedioVenta = g.Count() > 0 ? g.Sum(v => v.Total) / g.Count() : 0
                            })
                            .OrderBy(p => p.Periodo)
                            .ToList();
                        break;

                    case "año":
                        ventasPorPeriodo = ventas
                            .GroupBy(v => v.FechaVenta.Year)
                            .Select(g => new VentaPorPeriodoDto
                            {
                                Periodo = g.Key.ToString(),
                                PeriodoTexto = g.Key.ToString(),
                                CantidadVentas = g.Count(),
                                MontoTotal = g.Sum(v => v.Total),
                                PromedioVenta = g.Count() > 0 ? g.Sum(v => v.Total) / g.Count() : 0
                            })
                            .OrderBy(p => p.Periodo)
                            .ToList();
                        break;

                    default: // mes
                        ventasPorPeriodo = ventas
                            .GroupBy(v => new { v.FechaVenta.Year, v.FechaVenta.Month })
                            .Select(g => new VentaPorPeriodoDto
                            {
                                Periodo = $"{g.Key.Year}-{g.Key.Month:D2}",
                                PeriodoTexto = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy", new CultureInfo("es-ES")),
                                CantidadVentas = g.Count(),
                                MontoTotal = g.Sum(v => v.Total),
                                PromedioVenta = g.Count() > 0 ? g.Sum(v => v.Total) / g.Count() : 0
                            })
                            .OrderBy(p => p.Periodo)
                            .ToList();
                        break;
                }

                // Productos más vendidos en el período
                var productosMasVendidos = await ObtenerProductosMasVendidosAsync(fechaInicio, fechaFin, 5);

                // Clientes frecuentes en el período
                var clientesFrecuentes = await ObtenerClientesFrecuentesAsync(fechaInicio, fechaFin, 5);

                return new ReporteVentasDto
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    TipoAgrupacion = agrupacion ?? "mes",
                    TotalVentas = totalVentas,
                    CantidadVentas = cantidadVentas,
                    PromedioVenta = Math.Round(promedioVenta, 2),
                    VentasPorPeriodo = ventasPorPeriodo,
                    ProductosMasVendidos = productosMasVendidos,
                    ClientesFrecuentes = clientesFrecuentes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de ventas");
                throw new Exception($"Error al generar reporte: {ex.Message}");
            }
        }

        public async Task<List<ProductoVentaDto>> ObtenerProductosMasVendidosAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, int top = 10)
        {
            try
            {
                var query = _context.DetallesVenta
                    .Include(d => d.Producto)
                    .Include(d => d.Venta)
                    .AsQueryable();

                if (fechaInicio.HasValue)
                {
                    query = query.Where(d => d.Venta.FechaVenta >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(d => d.Venta.FechaVenta <= fechaFin.Value);
                }

                return await query
                    .GroupBy(d => new { d.ProductoId, d.Producto.Nombre, d.Producto.ImagenPrincipal })
                    .Select(g => new ProductoVentaDto
                    {
                        ProductoId = g.Key.ProductoId,
                        NombreProducto = g.Key.Nombre,
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        MontoTotal = g.Sum(d => d.Subtotal),
                        NumeroVentas = g.Select(d => d.VentaId).Distinct().Count(),
                        PromedioVenta = g.Average(d => d.Subtotal),
                        ImagenProducto = g.Key.ImagenPrincipal
                    })
                    .OrderByDescending(p => p.CantidadVendida)
                    .Take(top)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                throw new Exception($"Error al obtener productos: {ex.Message}");
            }
        }

        public async Task<List<ClienteVentaDto>> ObtenerClientesFrecuentesAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, int top = 10)
        {
            try
            {
                var query = _context.Ventas.AsQueryable();

                if (fechaInicio.HasValue)
                {
                    query = query.Where(v => v.FechaVenta >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(v => v.FechaVenta <= fechaFin.Value);
                }

                return await query
                    .GroupBy(v => new { v.UsuarioId, v.NombreCliente, v.EmailCliente })
                    .Select(g => new ClienteVentaDto
                    {
                        UsuarioId = g.Key.UsuarioId,
                        NombreCliente = g.Key.NombreCliente,
                        EmailCliente = g.Key.EmailCliente,
                        NumeroCompras = g.Count(),
                        MontoTotal = g.Sum(v => v.Total),
                        PromedioCompra = g.Average(v => v.Total),
                        UltimaCompra = g.Max(v => v.FechaVenta)
                    })
                    .OrderByDescending(c => c.NumeroCompras)
                    .Take(top)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes frecuentes");
                throw new Exception($"Error al obtener clientes: {ex.Message}");
            }
        }

        public async Task<List<VentaPorMetodoPagoDto>> ObtenerVentasPorMetodoPagoAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _context.Ventas
                    .Where(v => !string.IsNullOrEmpty(v.MetodoPago))
                    .AsQueryable();

                if (fechaInicio.HasValue)
                {
                    query = query.Where(v => v.FechaVenta >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(v => v.FechaVenta <= fechaFin.Value);
                }

                var totalMonto = await query.SumAsync(v => v.Total);

                var datos = await query
                    .GroupBy(v => v.MetodoPago!)
                    .Select(g => new VentaPorMetodoPagoDto
                    {
                        MetodoPago = g.Key,
                        CantidadVentas = g.Count(),
                        MontoTotal = g.Sum(v => v.Total),
                        Porcentaje = 0 // Se calculará después
                    })
                    .OrderByDescending(x => x.MontoTotal)
                    .ToListAsync();

                // Calcular porcentajes
                foreach (var item in datos)
                {
                    item.Porcentaje = totalMonto > 0 ? Math.Round((item.MontoTotal / totalMonto) * 100, 2) : 0;
                }

                return datos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por método de pago");
                throw new Exception($"Error al obtener datos: {ex.Message}");
            }
        }

        public async Task<List<VentaPorEstadoDto>> ObtenerVentasPorEstadoAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _context.Ventas.AsQueryable();

                if (fechaInicio.HasValue)
                {
                    query = query.Where(v => v.FechaVenta >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(v => v.FechaVenta <= fechaFin.Value);
                }

                var totalMonto = await query.SumAsync(v => v.Total);

                var datos = await query
                    .GroupBy(v => v.EstadoVenta)
                    .Select(g => new VentaPorEstadoDto
                    {
                        Estado = g.Key,
                        CantidadVentas = g.Count(),
                        MontoTotal = g.Sum(v => v.Total),
                        Porcentaje = 0 // Se calculará después
                    })
                    .OrderByDescending(x => x.CantidadVentas)
                    .ToListAsync();

                // Calcular porcentajes
                foreach (var item in datos)
                {
                    item.Porcentaje = totalMonto > 0 ? Math.Round((item.MontoTotal / totalMonto) * 100, 2) : 0;
                }

                return datos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por estado");
                throw new Exception($"Error al obtener datos: {ex.Message}");
            }
        }

        #endregion

        #region Métodos auxiliares

        public async Task<string> GenerarNumeroVentaAsync()
        {
            try
            {
                var fecha = DateTime.Now;
                var ultimaVenta = await _context.Ventas
                    .Where(v => v.FechaVenta.Year == fecha.Year && v.FechaVenta.Month == fecha.Month)
                    .OrderByDescending(v => v.Id)
                    .FirstOrDefaultAsync();

                var consecutivo = 1;
                if (ultimaVenta != null)
                {
                    // Extraer el consecutivo del último número de venta del mes
                    var partes = ultimaVenta.NumeroVenta.Split('-');
                    if (partes.Length == 3 && int.TryParse(partes[2], out int ultimo))
                    {
                        consecutivo = ultimo + 1;
                    }
                }

                return $"VT-{fecha:yyyyMM}-{consecutivo:D4}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar número de venta");
                throw new Exception($"Error al generar número de venta: {ex.Message}");
            }
        }

        public async Task<decimal> ObtenerTotalVentasDelMesAsync()
        {
            try
            {
                var fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                return await _context.Ventas
                    .Where(v => v.FechaVenta >= fechaInicio)
                    .SumAsync(v => v.Total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de ventas del mes");
                throw new Exception($"Error al obtener total: {ex.Message}");
            }
        }

        public async Task<List<Venta>> ObtenerVentasDelMesAsync()
        {
            try
            {
                var fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                return await _context.Ventas
                    .Where(v => v.FechaVenta >= fechaInicio)
                    .Include(v => v.Usuario)
                    .Include(v => v.Detalles)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas del mes");
                throw new Exception($"Error al obtener ventas: {ex.Message}");
            }
        }

        #endregion

        #region Métodos heredados (compatibilidad)

        public async Task<List<Venta>> ObtenerVentasAsync()
        {
            return await _context.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<Venta?> ObtenerVentaPorIdAsync(int id)
        {
            return await _context.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Venta> CrearVentaAsync(Venta venta)
        {
            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();
            return venta;
        }

        public async Task<bool> ActualizarEstadoVentaAsync(int id, string estado)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null) return false;

            venta.EstadoVenta = estado;
            return await _context.SaveChangesAsync() > 0;
        }

        #endregion
    }
}   