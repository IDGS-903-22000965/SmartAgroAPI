using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public class ComentarioService : IComentarioService
    {
        private readonly SmartAgroDbContext _context;

        public ComentarioService(SmartAgroDbContext context)
        {
            _context = context;
        }

        public async Task<List<Comentario>> ObtenerComentariosAsync()
        {
            return await _context.Comentarios
                .Include(c => c.Usuario)
                .Include(c => c.Producto)
                .OrderByDescending(c => c.FechaComentario)
                .ToListAsync();
        }

        public async Task<List<Comentario>> ObtenerComentariosPendientesAsync()
        {
            return await _context.Comentarios
                .Where(c => !c.Aprobado && c.Activo)
                .Include(c => c.Usuario)
                .Include(c => c.Producto)
                .OrderByDescending(c => c.FechaComentario)
                .ToListAsync();
        }

        public async Task<bool> AprobarComentarioAsync(int id)
        {
            var comentario = await _context.Comentarios.FindAsync(id);
            if (comentario == null) return false;

            comentario.Aprobado = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RechazarComentarioAsync(int id)
        {
            var comentario = await _context.Comentarios.FindAsync(id);
            if (comentario == null) return false;

            comentario.Activo = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ResponderComentarioAsync(int id, string respuesta)
        {
            var comentario = await _context.Comentarios.FindAsync(id);
            if (comentario == null) return false;

            comentario.RespuestaAdmin = respuesta;
            comentario.FechaRespuesta = DateTime.Now;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Comentario>> ObtenerComentariosPorProductoAsync(int productoId)
        {
            return await _context.Comentarios
                .Where(c => c.ProductoId == productoId && c.Aprobado && c.Activo)
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaComentario)
                .ToListAsync();
        }
    }
}
