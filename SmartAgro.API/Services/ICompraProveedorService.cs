// SmartAgro.API/Services/ICompraProveedorService.cs
using SmartAgro.Models.DTOs.ComprasProveedores;
using SmartAgro.Models.DTOs;

namespace SmartAgro.API.Services
{
    public interface ICompraProveedorService
    {
        Task<PaginatedComprasDto> ObtenerComprasAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, int? proveedorId = null, string? estado = null);
        Task<CompraProveedorDetailsDto?> ObtenerCompraPorIdAsync(int id);
        Task<ServiceResult> CrearCompraAsync(CreateCompraProveedorDto createCompraDto);
        Task<ServiceResult> ActualizarCompraAsync(int id, UpdateCompraProveedorDto updateCompraDto);
        Task<ServiceResult> EliminarCompraAsync(int id);
        Task<ServiceResult> CambiarEstadoCompraAsync(int id, string nuevoEstado);
        Task<CompraStatsDto> ObtenerEstadisticasAsync();
        Task<string> GenerarNumeroCompraAsync();
    }
}