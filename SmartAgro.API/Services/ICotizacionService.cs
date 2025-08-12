using SmartAgro.Models.DTOs;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public interface ICotizacionService
    {
        Task<Cotizacion> CrearCotizacionAsync(CotizacionRequestDto request, string? usuarioId = null);
        Task<List<CotizacionResponseDto>> ObtenerCotizacionesAsync();
        Task<CotizacionResponseDto?> ObtenerCotizacionPorIdAsync(int id);
        Task<List<CotizacionResponseDto>> ObtenerCotizacionesPorUsuarioAsync(string usuarioId);
        Task<bool> ActualizarEstadoCotizacionAsync(int id, string estado);
        Task<decimal> CalcularCostoCotizacionAsync(CotizacionRequestDto request);
    }
}