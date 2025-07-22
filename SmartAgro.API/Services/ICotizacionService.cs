using SmartAgro.Models.DTOs;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public interface ICotizacionService
    {
        Task<Cotizacion> CrearCotizacionAsync(CotizacionRequestDto request);
        Task<List<Cotizacion>> ObtenerCotizacionesAsync();
        Task<Cotizacion?> ObtenerCotizacionPorIdAsync(int id);
        Task<List<Cotizacion>> ObtenerCotizacionesPorUsuarioAsync(string usuarioId);
        Task<bool> ActualizarEstadoCotizacionAsync(int id, string estado);
        Task<decimal> CalcularCostoCotizacionAsync(CotizacionRequestDto request);
    }
}