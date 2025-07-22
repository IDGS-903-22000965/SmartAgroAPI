using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public interface IProveedorService
    {
        Task<List<Proveedor>> ObtenerProveedoresAsync();
        Task<Proveedor?> ObtenerProveedorPorIdAsync(int id);
        Task<Proveedor> CrearProveedorAsync(Proveedor proveedor);
        Task<bool> ActualizarProveedorAsync(Proveedor proveedor);
        Task<bool> EliminarProveedorAsync(int id);
        Task<List<Proveedor>> BuscarProveedoresAsync(string termino);
    }
}