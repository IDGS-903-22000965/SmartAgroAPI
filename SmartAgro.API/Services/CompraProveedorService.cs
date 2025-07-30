// SmartAgro.API/Services/CompraProveedorService.cs
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.DTOs.ComprasProveedores;
using SmartAgro.Models.DTOs;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public class CompraProveedorService : ICompraProveedorService
    {
        private readonly SmartAgroDbContext _context;

        public CompraProveedorService(SmartAgroDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedComprasDto> ObtenerComprasAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, int? proveedorId = null, string? estado = null)
        {
            try
            {
                var query = _context.ComprasProveedores
                    .Include(c => c.Proveedor)
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.MateriaPrima)
                    .AsQueryable();

                // Filtros
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(c => c.NumeroCompra.Contains(searchTerm) ||
                                           c.Proveedor.Nombre.Contains(searchTerm) ||
                                           (c.Proveedor.RazonSocial != null && c.Proveedor.RazonSocial.Contains(searchTerm)));
                }

                if (proveedorId.HasValue)
                {
                    query = query.Where(c => c.ProveedorId == proveedorId.Value);
                }

                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(c => c.Estado == estado);
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var compras = await query
                    .OrderByDescending(c => c.FechaCompra)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CompraProveedorListDto
                    {
                        Id = c.Id,
                        NumeroCompra = c.NumeroCompra,
                        ProveedorNombre = c.Proveedor.Nombre,
                        Total = c.Total,
                        FechaCompra = c.FechaCompra,
                        Estado = c.Estado,
                        CantidadItems = c.Detalles.Count(),
                        Observaciones = c.Observaciones
                    })
                    .ToListAsync();

                return new PaginatedComprasDto
                {
                    Compras = compras,
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
                throw new Exception($"Error al obtener compras: {ex.Message}");
            }
        }

        public async Task<CompraProveedorDetailsDto?> ObtenerCompraPorIdAsync(int id)
        {
            try
            {
                var compra = await _context.ComprasProveedores
                    .Include(c => c.Proveedor)
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.MateriaPrima)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (compra == null) return null;

                return new CompraProveedorDetailsDto
                {
                    Id = compra.Id,
                    NumeroCompra = compra.NumeroCompra,
                    ProveedorId = compra.ProveedorId,
                    ProveedorNombre = compra.Proveedor.Nombre,
                    ProveedorRazonSocial = compra.Proveedor.RazonSocial ?? "",
                    Total = compra.Total,
                    FechaCompra = compra.FechaCompra,
                    Estado = compra.Estado,
                    Observaciones = compra.Observaciones,
                    Detalles = compra.Detalles.Select(d => new DetalleCompraDto
                    {
                        Id = d.Id,
                        MateriaPrimaId = d.MateriaPrimaId,
                        MateriaPrimaNombre = d.MateriaPrima.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener compra: {ex.Message}");
            }
        }

        public async Task<ServiceResult> CrearCompraAsync(CreateCompraProveedorDto createCompraDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var proveedor = await _context.Proveedores.FindAsync(createCompraDto.ProveedorId);
                if (proveedor == null)
                    return ServiceResult.ErrorResult("Proveedor no encontrado");

                var numeroCompra = await GenerarNumeroCompraAsync();

                var compra = new CompraProveedor
                {
                    NumeroCompra = numeroCompra,
                    ProveedorId = createCompraDto.ProveedorId,
                    FechaCompra = createCompraDto.FechaCompra,
                    Estado = "Pendiente",
                    Observaciones = createCompraDto.Observaciones,
                    Total = 0
                };

                _context.ComprasProveedores.Add(compra);
                await _context.SaveChangesAsync();

                decimal totalCompra = 0;
                foreach (var detalle in createCompraDto.Detalles)
                {
                    var materiaPrima = await _context.MateriasPrimas.FindAsync(detalle.MateriaPrimaId);
                    if (materiaPrima == null)
                        return ServiceResult.ErrorResult($"Materia prima ID {detalle.MateriaPrimaId} no encontrada");

                    var subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                    totalCompra += subtotal;

                    var detalleCompra = new DetalleCompraProveedor
                    {
                        CompraProveedorId = compra.Id,
                        MateriaPrimaId = detalle.MateriaPrimaId,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario,
                        Subtotal = subtotal,
                        Notas = detalle.Notas
                    };

                    _context.DetallesCompraProveedor.Add(detalleCompra);

                    // ✅ REGISTRAR MOVIMIENTO DE STOCK
                    var movimiento = new MovimientoStock
                    {
                        MateriaPrimaId = detalle.MateriaPrimaId,
                        Tipo = "Entrada",
                        Cantidad = detalle.Cantidad,
                        CostoUnitario = detalle.PrecioUnitario,
                        Referencia = numeroCompra,
                        Observaciones = $"Compra a proveedor: {proveedor.Nombre}",
                        Fecha = createCompraDto.FechaCompra
                    };

                    _context.MovimientosStock.Add(movimiento);

                    // ✅ ACTUALIZAR STOCK FÍSICO DE LA MATERIA PRIMA
                    materiaPrima.Stock += (int)detalle.Cantidad;

                    // ✅ ACTUALIZAR COSTO UNITARIO CON PROMEDIO PONDERADO
                    var stockAnterior = materiaPrima.Stock - (int)detalle.Cantidad;
                    if (stockAnterior > 0)
                    {
                        // Costo promedio ponderado
                        var valorAnterior = stockAnterior * materiaPrima.CostoUnitario;
                        var valorNuevo = detalle.Cantidad * detalle.PrecioUnitario;
                        materiaPrima.CostoUnitario = (valorAnterior + valorNuevo) / materiaPrima.Stock;
                    }
                    else
                    {
                        // Si no había stock anterior, usar el costo de la compra
                        materiaPrima.CostoUnitario = detalle.PrecioUnitario;
                    }
                    _context.Entry(materiaPrima).State = EntityState.Modified;

                }

                compra.Total = totalCompra;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.SuccessResult($"Compra {numeroCompra} creada exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult.ErrorResult($"Error al crear compra: {ex.Message}");
            }
        }

        public async Task<ServiceResult> ActualizarCompraAsync(int id, UpdateCompraProveedorDto updateCompraDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var compra = await _context.ComprasProveedores
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.MateriaPrima)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (compra == null)
                {
                    return ServiceResult.ErrorResult("Compra no encontrada");
                }

                if (compra.Estado == "Recibido")
                {
                    return ServiceResult.ErrorResult("No se puede modificar una compra ya recibida");
                }

                // ✅ REVERTIR MOVIMIENTOS Y STOCK ANTERIORES
                foreach (var detalleAnterior in compra.Detalles)
                {
                    // Revertir stock
                    detalleAnterior.MateriaPrima.Stock -= (int)detalleAnterior.Cantidad;

                    // Eliminar movimientos de stock relacionados
                    var movimientosAnteriores = await _context.MovimientosStock
                        .Where(m => m.MateriaPrimaId == detalleAnterior.MateriaPrimaId &&
                                   m.Referencia == compra.NumeroCompra)
                        .ToListAsync();

                    _context.MovimientosStock.RemoveRange(movimientosAnteriores);
                }

                // Actualizar datos básicos
                compra.ProveedorId = updateCompraDto.ProveedorId;
                compra.FechaCompra = updateCompraDto.FechaCompra;
                compra.Observaciones = updateCompraDto.Observaciones;

                // Eliminar detalles existentes
                _context.DetallesCompraProveedor.RemoveRange(compra.Detalles);
                await _context.SaveChangesAsync();

                // Crear nuevos detalles
                decimal totalCompra = 0;
                foreach (var detalle in updateCompraDto.Detalles)
                {
                    var materiaPrima = await _context.MateriasPrimas.FindAsync(detalle.MateriaPrimaId);
                    if (materiaPrima == null)
                        return ServiceResult.ErrorResult($"Materia prima ID {detalle.MateriaPrimaId} no encontrada");

                    var subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                    totalCompra += subtotal;

                    var detalleCompra = new DetalleCompraProveedor
                    {
                        CompraProveedorId = compra.Id,
                        MateriaPrimaId = detalle.MateriaPrimaId,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario,
                        Subtotal = subtotal,
                        Notas = detalle.Notas
                    };

                    _context.DetallesCompraProveedor.Add(detalleCompra);

                    // ✅ REGISTRAR NUEVO MOVIMIENTO DE STOCK
                    var movimiento = new MovimientoStock
                    {
                        MateriaPrimaId = detalle.MateriaPrimaId,
                        Tipo = "Entrada",
                        Cantidad = detalle.Cantidad,
                        CostoUnitario = detalle.PrecioUnitario,
                        Referencia = compra.NumeroCompra,
                        Observaciones = $"Compra actualizada - Proveedor: {compra.Proveedor.Nombre}",
                        Fecha = updateCompraDto.FechaCompra
                    };

                    _context.MovimientosStock.Add(movimiento);

                    // ✅ ACTUALIZAR STOCK FÍSICO
                    materiaPrima.Stock += (int)detalle.Cantidad;
                }

                compra.Total = totalCompra;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return ServiceResult.SuccessResult("Compra actualizada exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult.ErrorResult($"Error al actualizar compra: {ex.Message}");
            }
        }

        public async Task<ServiceResult> EliminarCompraAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var compra = await _context.ComprasProveedores
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.MateriaPrima)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (compra == null)
                {
                    return ServiceResult.ErrorResult("Compra no encontrada");
                }

                if (compra.Estado == "Recibido")
                {
                    return ServiceResult.ErrorResult("No se puede eliminar una compra ya recibida");
                }

                // ✅ REVERTIR STOCK SI LA COMPRA ESTÁ PENDIENTE
                foreach (var detalle in compra.Detalles)
                {
                    // Revertir stock
                    detalle.MateriaPrima.Stock -= (int)detalle.Cantidad;

                    // Eliminar movimientos de stock relacionados
                    var movimientos = await _context.MovimientosStock
                        .Where(m => m.MateriaPrimaId == detalle.MateriaPrimaId &&
                                   m.Referencia == compra.NumeroCompra)
                        .ToListAsync();

                    _context.MovimientosStock.RemoveRange(movimientos);
                }

                _context.ComprasProveedores.Remove(compra);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.SuccessResult("Compra eliminada exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult.ErrorResult($"Error al eliminar compra: {ex.Message}");
            }
        }

        public async Task<ServiceResult> CambiarEstadoCompraAsync(int id, string nuevoEstado)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var compra = await _context.ComprasProveedores
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.MateriaPrima)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (compra == null)
                {
                    return ServiceResult.ErrorResult("Compra no encontrada");
                }

                var estadosValidos = new[] { "Pendiente", "Recibido", "Cancelado" };
                if (!estadosValidos.Contains(nuevoEstado))
                {
                    return ServiceResult.ErrorResult("Estado no válido");
                }

                var estadoAnterior = compra.Estado;
                compra.Estado = nuevoEstado;

                // ✅ LÓGICA ESPECIAL PARA CAMBIOS DE ESTADO
                if (estadoAnterior == "Pendiente" && nuevoEstado == "Cancelado")
                {
                    // Revertir stock al cancelar
                    foreach (var detalle in compra.Detalles)
                    {
                        detalle.MateriaPrima.Stock -= (int)detalle.Cantidad;

                        // Registrar movimiento de cancelación
                        var movimientoCancelacion = new MovimientoStock
                        {
                            MateriaPrimaId = detalle.MateriaPrimaId,
                            Tipo = "Salida",
                            Cantidad = detalle.Cantidad,
                            CostoUnitario = detalle.PrecioUnitario,
                            Referencia = $"CANCEL-{compra.NumeroCompra}",
                            Observaciones = "Cancelación de compra",
                            Fecha = DateTime.Now
                        };

                        _context.MovimientosStock.Add(movimientoCancelacion);
                    }
                }
                else if (estadoAnterior == "Cancelado" && nuevoEstado == "Pendiente")
                {
                    // Restaurar stock al reactivar
                    foreach (var detalle in compra.Detalles)
                    {
                        detalle.MateriaPrima.Stock += (int)detalle.Cantidad;

                        // Eliminar movimientos de cancelación
                        var movimientosCancelacion = await _context.MovimientosStock
                            .Where(m => m.Referencia == $"CANCEL-{compra.NumeroCompra}")
                            .ToListAsync();

                        _context.MovimientosStock.RemoveRange(movimientosCancelacion);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.SuccessResult($"Estado cambiado a {nuevoEstado}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult.ErrorResult($"Error al cambiar estado: {ex.Message}");
            }
        }

        public async Task<CompraStatsDto> ObtenerEstadisticasAsync()
        {
            try
            {
                var totalCompras = await _context.ComprasProveedores.CountAsync();
                var comprasPendientes = await _context.ComprasProveedores.CountAsync(c => c.Estado == "Pendiente");
                var comprasRecibidas = await _context.ComprasProveedores.CountAsync(c => c.Estado == "Recibido");
                var comprasCanceladas = await _context.ComprasProveedores.CountAsync(c => c.Estado == "Cancelado");

                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var comprasEsteMes = await _context.ComprasProveedores.CountAsync(c => c.FechaCompra >= inicioMes);

                var totalGastado = await _context.ComprasProveedores
                    .Where(c => c.Estado == "Recibido")
                    .SumAsync(c => (decimal?)c.Total) ?? 0;

                var gastoEsteMes = await _context.ComprasProveedores
                    .Where(c => c.FechaCompra >= inicioMes && c.Estado == "Recibido")
                    .SumAsync(c => (decimal?)c.Total) ?? 0;

                return new CompraStatsDto
                {
                    TotalCompras = totalCompras,
                    ComprasPendientes = comprasPendientes,
                    ComprasRecibidas = comprasRecibidas,
                    ComprasCanceladas = comprasCanceladas,
                    ComprasEsteMes = comprasEsteMes,
                    TotalGastado = totalGastado,
                    GastoEsteMes = gastoEsteMes
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener estadísticas: {ex.Message}");
            }
        }

        public async Task<string> GenerarNumeroCompraAsync()
        {
            var ultimaCompra = await _context.ComprasProveedores
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            var numero = ultimaCompra?.Id + 1 ?? 1;
            return $"CP-{DateTime.Now:yyyyMM}-{numero:D4}";
        }
    }
}