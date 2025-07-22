using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public interface IMateriaPrimaService
    {
        Task<List<MateriaPrima>> ObtenerMateriasPrimasAsync();
        Task<MateriaPrima?> ObtenerMateriaPrimaPorIdAsync(int id);
        Task<MateriaPrima> CrearMateriaPrimaAsync(MateriaPrima materiaPrima);
        Task<bool> ActualizarMateriaPrimaAsync(MateriaPrima materiaPrima);
        Task<bool> EliminarMateriaPrimaAsync(int id);
        Task<List<MateriaPrima>> ObtenerMateriasPrimasPorProveedorAsync(int proveedorId);
        Task<bool> ActualizarStockAsync(int id, int nuevoStock);
    }
}