// SmartAgro.API/Services/ICompraProveedorService.cs
using SmartAgro.Models.DTOs.ComprasProveedores;
using SmartAgro.Models.DTOs;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public interface ICompraProveedorService
    {
        /// <summary>
        /// Obtiene todas las compras con paginación y filtros
        /// </summary>
        Task<PaginatedComprasDto> ObtenerComprasAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, int? proveedorId = null, string? estado = null);

        /// <summary>
        /// Obtiene una compra específica por ID
        /// </summary>
        Task<CompraProveedorDetailsDto?> ObtenerCompraPorIdAsync(int id);

        /// <summary>
        /// Crea una nueva compra
        /// </summary>
        Task<ServiceResult> CrearCompraAsync(CreateCompraProveedorDto createCompraDto);

        /// <summary>
        /// Actualiza una compra existente
        /// </summary>
        Task<ServiceResult> ActualizarCompraAsync(int id, UpdateCompraProveedorDto updateCompraDto);

        /// <summary>
        /// Elimina una compra
        /// </summary>
        Task<ServiceResult> EliminarCompraAsync(int id);

        /// <summary>
        /// Cambia el estado de una compra
        /// </summary>
        Task<ServiceResult> CambiarEstadoCompraAsync(int id, string nuevoEstado);

        /// <summary>
        /// Obtiene estadísticas de compras
        /// </summary>
        Task<CompraStatsDto> ObtenerEstadisticasAsync();

        /// <summary>
        /// Genera número de compra único
        /// </summary>
        Task<string> GenerarNumeroCompraAsync();
    }
}