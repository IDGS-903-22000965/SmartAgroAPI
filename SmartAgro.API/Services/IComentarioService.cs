using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public interface IComentarioService
    {
        Task<List<Comentario>> ObtenerComentariosAsync();
        Task<List<Comentario>> ObtenerComentariosPendientesAsync();
        Task<bool> AprobarComentarioAsync(int id);
        Task<bool> RechazarComentarioAsync(int id);
        Task<bool> ResponderComentarioAsync(int id, string respuesta);
        Task<List<Comentario>> ObtenerComentariosPorProductoAsync(int productoId);
    }
}