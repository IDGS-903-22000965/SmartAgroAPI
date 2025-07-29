// SmartAgro.API/Services/IMateriaPrimaService.cs
using SmartAgro.Data;
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
        Task<bool> ActualizarStockAsync(int id, int nuevoStock);
        Task<List<MateriaPrima>> ObtenerPorProveedorAsync(int proveedorId);
        Task<List<MateriaPrima>> ObtenerBajoStockAsync();
        Task<List<MateriaPrima>> BuscarMateriasPrimasAsync(string termino);
    }
}

