using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public class VentaService : IVentaService
    {
        private readonly SmartAgroDbContext _context;

        public VentaService(SmartAgroDbContext context)
        {
            _context = context;
        }

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

        public async Task<List<Venta>> ObtenerVentasPorUsuarioAsync(string usuarioId)
        {
            return await _context.Ventas
                .Where(v => v.UsuarioId == usuarioId)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<decimal> ObtenerTotalVentasDelMesAsync()
        {
            var fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio)
                .SumAsync(v => v.Total);
        }

        public async Task<List<Venta>> ObtenerVentasDelMesAsync()
        {
            var fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio)
                .Include(v => v.Usuario)
                .Include(v => v.Detalles)
                .ToListAsync();
        }
    }
}