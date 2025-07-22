using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public class MateriaPrimaService : IMateriaPrimaService
    {
        private readonly SmartAgroDbContext _context;

        public MateriaPrimaService(SmartAgroDbContext context)
        {
            _context = context;
        }

        public async Task<List<MateriaPrima>> ObtenerMateriasPrimasAsync()
        {
            return await _context.MateriasPrimas
                .Where(m => m.Activo)
                .Include(m => m.Proveedor)
                .ToListAsync();
        }

        public async Task<MateriaPrima?> ObtenerMateriaPrimaPorIdAsync(int id)
        {
            return await _context.MateriasPrimas
                .Include(m => m.Proveedor)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MateriaPrima> CrearMateriaPrimaAsync(MateriaPrima materiaPrima)
        {
            _context.MateriasPrimas.Add(materiaPrima);
            await _context.SaveChangesAsync();
            return materiaPrima;
        }

        public async Task<bool> ActualizarMateriaPrimaAsync(MateriaPrima materiaPrima)
        {
            _context.MateriasPrimas.Update(materiaPrima);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarMateriaPrimaAsync(int id)
        {
            var materiaPrima = await _context.MateriasPrimas.FindAsync(id);
            if (materiaPrima == null) return false;

            materiaPrima.Activo = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<MateriaPrima>> ObtenerMateriasPrimasPorProveedorAsync(int proveedorId)
        {
            return await _context.MateriasPrimas
                .Where(m => m.ProveedorId == proveedorId && m.Activo)
                .ToListAsync();
        }

        public async Task<bool> ActualizarStockAsync(int id, int nuevoStock)
        {
            var materiaPrima = await _context.MateriasPrimas.FindAsync(id);
            if (materiaPrima == null) return false;

            materiaPrima.Stock = nuevoStock;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
