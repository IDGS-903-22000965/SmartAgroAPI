using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public interface IVentaService
    {
        Task<List<Venta>> ObtenerVentasAsync();
        Task<Venta?> ObtenerVentaPorIdAsync(int id);
        Task<Venta> CrearVentaAsync(Venta venta);
        Task<bool> ActualizarEstadoVentaAsync(int id, string estado);
        Task<List<Venta>> ObtenerVentasPorUsuarioAsync(string usuarioId);
        Task<decimal> ObtenerTotalVentasDelMesAsync();
        Task<List<Venta>> ObtenerVentasDelMesAsync();
    }
}
