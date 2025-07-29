using SmartAgro.Models.DTOs;
using SmartAgro.Models.DTOs.Ventas;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public interface IVentaService
    {
        // Métodos básicos CRUD
        Task<PaginatedVentasDto> ObtenerVentasPaginadasAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? estado = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            string? metodoPago = null);

        Task<VentaDetalleDto?> ObtenerVentaDetalleAsync(int id);
        Task<List<VentaListDto>> ObtenerVentasPorUsuarioAsync(string usuarioId);
        Task<ServiceResult> CrearVentaAsync(CreateVentaDto createVentaDto);
        Task<ServiceResult> CrearVentaDesdeCotizacionAsync(int cotizacionId, CreateVentaFromCotizacionDto ventaDto);
        Task<ServiceResult> ActualizarEstadoVentaAsync(int id, ActualizarEstadoVentaDto estadoDto);

        // Métodos de estadísticas y reportes
        Task<VentaStatsDto> ObtenerEstadisticasVentasAsync();
        Task<ReporteVentasDto> GenerarReporteVentasAsync(DateTime fechaInicio, DateTime fechaFin, string? agrupacion = "mes");
        Task<List<ProductoVentaDto>> ObtenerProductosMasVendidosAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, int top = 10);
        Task<List<ClienteVentaDto>> ObtenerClientesFrecuentesAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, int top = 10);
        Task<List<VentaPorMetodoPagoDto>> ObtenerVentasPorMetodoPagoAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);
        Task<List<VentaPorEstadoDto>> ObtenerVentasPorEstadoAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);

        // Métodos auxiliares
        Task<string> GenerarNumeroVentaAsync();
        Task<decimal> ObtenerTotalVentasDelMesAsync();
        Task<List<Venta>> ObtenerVentasDelMesAsync();

        // Métodos heredados del servicio anterior (para compatibilidad)
        Task<List<Venta>> ObtenerVentasAsync();
        Task<Venta?> ObtenerVentaPorIdAsync(int id);
        Task<Venta> CrearVentaAsync(Venta venta);
        Task<bool> ActualizarEstadoVentaAsync(int id, string estado);
    }
}