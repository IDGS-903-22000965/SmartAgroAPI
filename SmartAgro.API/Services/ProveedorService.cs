using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly SmartAgroDbContext _context;

        public ProveedorService(SmartAgroDbContext context)
        {
            _context = context;
        }

        public async Task<List<Proveedor>> ObtenerProveedoresAsync()
        {
            return await _context.Proveedores
                .Where(p => p.Activo)
                .Include(p => p.MateriasPrimas)
                .ToListAsync();
        }

        public async Task<Proveedor?> ObtenerProveedorPorIdAsync(int id)
        {
            return await _context.Proveedores
                .Include(p => p.MateriasPrimas)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Proveedor> CrearProveedorAsync(Proveedor proveedor)
        {
            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();
            return proveedor;
        }

        public async Task<bool> ActualizarProveedorAsync(Proveedor proveedor)
        {
            _context.Proveedores.Update(proveedor);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarProveedorAsync(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return false;

            proveedor.Activo = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Proveedor>> BuscarProveedoresAsync(string termino)
        {
            return await _context.Proveedores
                .Where(p => p.Activo &&
                           (p.Nombre.Contains(termino) ||
                            p.RazonSocial.Contains(termino)))
                .ToListAsync();
        }
    }
}
