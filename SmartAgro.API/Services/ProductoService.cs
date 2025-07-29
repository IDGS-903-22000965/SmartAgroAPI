using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public class ProductoService : IProductoService
    {
        private readonly ICosteoFifoService _costeoFifoService;

        private readonly SmartAgroDbContext _context;

        public ProductoService(SmartAgroDbContext context)
        {
            _context = context;
        }
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
                .ToListAsync();
        }

        public async Task<Producto?> ObtenerProductoPorIdAsync(int id)
        {
            return await _context.Productos
                .Include(p => p.Comentarios)
                .Include(p => p.ProductoMateriasPrimas)
                .ThenInclude(pm => pm.MateriaPrima)
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
                            p.Descripcion!.Contains(termino)))
                .ToListAsync();
        }

        public async Task<decimal> CalcularPrecioCostoAsync(int productoId)
        {
            var materiales = await _context.ProductoMateriasPrimas
                .Where(pm => pm.ProductoId == productoId)
                .ToListAsync();

            decimal costoTotal = 0;

            foreach (var material in materiales)
            {
                var costoMaterial = await _costeoFifoService.ObtenerCostoSalidaFifoAsync(
                    material.MateriaPrimaId,
                    material.CantidadRequerida);

                costoTotal += costoMaterial;
            }

            return costoTotal;
        }
    }
}