using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public interface IProductoService
    {
        Task<List<Producto>> ObtenerProductosAsync();
        Task<Producto?> ObtenerProductoPorIdAsync(int id);
        Task<Producto> CrearProductoAsync(Producto producto);
        Task<bool> ActualizarProductoAsync(Producto producto);
        Task<bool> EliminarProductoAsync(int id);
        Task<List<Producto>> BuscarProductosAsync(string termino);
        Task<decimal> CalcularPrecioCostoAsync(int productoId);
    }
}