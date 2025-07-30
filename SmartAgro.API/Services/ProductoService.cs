// SmartAgro.API/Services/ProductoService.cs
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.Entities;
using SmartAgro.Models.DTOs;

namespace SmartAgro.API.Services
{
    public class ProductoService : IProductoService
    {
        private readonly SmartAgroDbContext _context;
        private readonly ICosteoFifoService _costeoFifoService;

        // ✅ UN SOLO CONSTRUCTOR
        public ProductoService(SmartAgroDbContext context, ICosteoFifoService costeoFifoService)
        {
            _context = context;
            _costeoFifoService = costeoFifoService;
        }

        public async Task<List<Producto>> ObtenerProductosAsync()
        {
            return await _context.Productos
                .Where(p => p.Activo)
                .Include(p => p.Comentarios)
                .Include(p => p.ProductoMateriasPrimas)
                    .ThenInclude(pm => pm.MateriaPrima)
                .ToListAsync();
        }

        public async Task<Producto?> ObtenerProductoPorIdAsync(int id)
        {
            return await _context.Productos
                .Include(p => p.Comentarios)
                .Include(p => p.ProductoMateriasPrimas)
                    .ThenInclude(pm => pm.MateriaPrima)
                        .ThenInclude(mp => mp.Proveedor)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Producto> CrearProductoAsync(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return producto;
        }

        public async Task<bool> ActualizarProductoAsync(Producto producto)
        {
            _context.Productos.Update(producto);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarProductoAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return false;

            producto.Activo = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Producto>> BuscarProductosAsync(string termino)
        {
            return await _context.Productos
                .Where(p => p.Activo &&
                           (p.Nombre.Contains(termino) ||
                            (p.Descripcion != null && p.Descripcion.Contains(termino))))
                .Include(p => p.ProductoMateriasPrimas)
                    .ThenInclude(pm => pm.MateriaPrima)
                .ToListAsync();
        }

        // ✅ MÉTODOS PARA GESTIONAR RECETAS/EXPLOSIÓN DE MATERIALES
        public async Task<bool> AgregarMateriaPrimaAsync(int productoId, ProductoMateriaPrimaCreateDto materiaPrimaDto)
        {
            try
            {
                // Verificar que el producto existe
                var producto = await _context.Productos.FindAsync(productoId);
                if (producto == null) return false;

                // Verificar que la materia prima existe
                var materiaPrima = await _context.MateriasPrimas.FindAsync(materiaPrimaDto.MateriaPrimaId);
                if (materiaPrima == null) return false;

                // Verificar que no existe ya esta relación
                var existeRelacion = await _context.ProductoMateriasPrimas
                    .AnyAsync(pm => pm.ProductoId == productoId && pm.MateriaPrimaId == materiaPrimaDto.MateriaPrimaId);

                if (existeRelacion) return false;

                // Calcular costos
                var costoUnitario = materiaPrima.CostoUnitario;
                var costoTotal = materiaPrimaDto.CantidadRequerida * costoUnitario;

                var productoMateriaPrima = new ProductoMateriaPrima
                {
                    ProductoId = productoId,
                    MateriaPrimaId = materiaPrimaDto.MateriaPrimaId,
                    CantidadRequerida = materiaPrimaDto.CantidadRequerida,
                    CostoUnitario = costoUnitario,
                    CostoTotal = costoTotal,
                    Notas = materiaPrimaDto.Notas
                };

                _context.ProductoMateriasPrimas.Add(productoMateriaPrima);
                await _context.SaveChangesAsync();

                // Recalcular precio del producto
                await RecalcularPrecioProductoAsync(productoId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActualizarMateriaPrimaAsync(int productoId, int materiaPrimaId, ProductoMateriaPrimaCreateDto materiaPrimaDto)
        {
            try
            {
                var productoMateriaPrima = await _context.ProductoMateriasPrimas
                    .Include(pm => pm.MateriaPrima)
                    .FirstOrDefaultAsync(pm => pm.ProductoId == productoId && pm.MateriaPrimaId == materiaPrimaId);

                if (productoMateriaPrima == null) return false;

                // Actualizar datos
                productoMateriaPrima.CantidadRequerida = materiaPrimaDto.CantidadRequerida;
                productoMateriaPrima.CostoUnitario = productoMateriaPrima.MateriaPrima.CostoUnitario;
                productoMateriaPrima.CostoTotal = materiaPrimaDto.CantidadRequerida * productoMateriaPrima.CostoUnitario;
                productoMateriaPrima.Notas = materiaPrimaDto.Notas;

                await _context.SaveChangesAsync();

                // Recalcular precio del producto
                await RecalcularPrecioProductoAsync(productoId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EliminarMateriaPrimaAsync(int productoId, int materiaPrimaId)
        {
            try
            {
                var productoMateriaPrima = await _context.ProductoMateriasPrimas
                    .FirstOrDefaultAsync(pm => pm.ProductoId == productoId && pm.MateriaPrimaId == materiaPrimaId);

                if (productoMateriaPrima == null) return false;

                _context.ProductoMateriasPrimas.Remove(productoMateriaPrima);
                await _context.SaveChangesAsync();

                // Recalcular precio del producto
                await RecalcularPrecioProductoAsync(productoId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<decimal> CalcularPrecioCostoAsync(int productoId)
        {
            var materiales = await _context.ProductoMateriasPrimas
                .Where(pm => pm.ProductoId == productoId)
                .ToListAsync();

            decimal costoTotal = 0;

            foreach (var material in materiales)
            {
                try
                {
                    var costoMaterial = await _costeoFifoService.ObtenerCostoSalidaFifoAsync(
                        material.MateriaPrimaId,
                        material.CantidadRequerida);
                    costoTotal += costoMaterial;
                }
                catch
                {
                    // Si falla FIFO, usar costo unitario actual
                    costoTotal += material.CostoTotal;
                }
            }

            return costoTotal;
        }

        public async Task<bool> RecalcularPrecioProductoAsync(int productoId)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(productoId);
                if (producto == null) return false;

                // Calcular costo total de materiales
                var costoMateriales = await CalcularPrecioCostoAsync(productoId);

                // Actualizar precio base con el costo de materiales + margen
                producto.PrecioBase = costoMateriales;
                producto.PrecioVenta = producto.PrecioBase * (1 + producto.PorcentajeGanancia / 100);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ProductoMateriaPrimaDetalleDto>> ObtenerRecetaProductoAsync(int productoId)
        {
            return await _context.ProductoMateriasPrimas
                .Where(pm => pm.ProductoId == productoId)
                .Include(pm => pm.MateriaPrima)
                    .ThenInclude(mp => mp.Proveedor)
                .Select(pm => new ProductoMateriaPrimaDetalleDto
                {
                    Id = pm.Id,
                    MateriaPrimaId = pm.MateriaPrimaId,
                    NombreMateriaPrima = pm.MateriaPrima.Nombre,
                    DescripcionMateriaPrima = pm.MateriaPrima.Descripcion,
                    CantidadRequerida = pm.CantidadRequerida,
                    UnidadMedida = pm.MateriaPrima.UnidadMedida,
                    CostoUnitario = pm.CostoUnitario,
                    CostoTotal = pm.CostoTotal,
                    StockDisponible = pm.MateriaPrima.Stock,
                    NombreProveedor = pm.MateriaPrima.Proveedor.Nombre,
                    Notas = pm.Notas
                })
                .ToListAsync();
        }

        public async Task<bool> ValidarStockParaProduccionAsync(int productoId, int cantidadAProcuir)
        {
            var receta = await ObtenerRecetaProductoAsync(productoId);

            foreach (var material in receta)
            {
                var cantidadNecesaria = material.CantidadRequerida * cantidadAProcuir;
                if (material.StockDisponible < cantidadNecesaria)
                {
                    return false;
                }
            }

            return true;
        }
    }
}