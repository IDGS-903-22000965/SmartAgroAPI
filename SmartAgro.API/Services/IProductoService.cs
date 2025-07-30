// SmartAgro.API/Services/IProductoService.cs
using SmartAgro.Models.Entities;
using SmartAgro.Models.DTOs;

namespace SmartAgro.API.Services
{
    public interface IProductoService
    {
        // Métodos básicos de productos
        Task<List<Producto>> ObtenerProductosAsync();
        Task<Producto?> ObtenerProductoPorIdAsync(int id);
        Task<Producto> CrearProductoAsync(Producto producto);
        Task<bool> ActualizarProductoAsync(Producto producto);
        Task<bool> EliminarProductoAsync(int id);
        Task<List<Producto>> BuscarProductosAsync(string termino);

        // Métodos para gestión de recetas/explosión de materiales
        Task<bool> AgregarMateriaPrimaAsync(int productoId, ProductoMateriaPrimaCreateDto materiaPrimaDto);
        Task<bool> ActualizarMateriaPrimaAsync(int productoId, int materiaPrimaId, ProductoMateriaPrimaCreateDto materiaPrimaDto);
        Task<bool> EliminarMateriaPrimaAsync(int productoId, int materiaPrimaId);
        Task<List<ProductoMateriaPrimaDetalleDto>> ObtenerRecetaProductoAsync(int productoId);

        // Métodos para cálculos y costeo
        Task<decimal> CalcularPrecioCostoAsync(int productoId);
        Task<bool> RecalcularPrecioProductoAsync(int productoId);
        Task<bool> ValidarStockParaProduccionAsync(int productoId, int cantidadAProducir);
    }
}