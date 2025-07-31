using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.DTOs.Users;

namespace SmartAgro.API.Services
{
    public class ClientService : IClientService
    {
        private readonly SmartAgroDbContext _context;
        private readonly ILogger<ClientService> _logger;

        public ClientService(SmartAgroDbContext context, ILogger<ClientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ClientStatsDto> GetClientStatsAsync(string userId)
        {
            try
            {
                var ventas = await _context.Ventas
                    .Where(v => v.UsuarioId == userId)
                    .ToListAsync();

                var comentarios = await _context.Comentarios
                    .Where(c => c.UsuarioId == userId)
                    .ToListAsync();

                var totalCompras = ventas.Count;
                var montoTotalGastado = ventas.Sum(v => v.Total);
                var comentariosRealizados = comentarios.Count;
                var ultimaCompra = ventas.Any() ? ventas.Max(v => v.FechaVenta) : (DateTime?)null;

                return new ClientStatsDto
                {
                    TotalCompras = totalCompras,
                    MontoTotalGastado = montoTotalGastado,
                    ComentariosRealizados = comentariosRealizados,
                    UltimaCompra = ultimaCompra
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del cliente {UserId}", userId);
                return new ClientStatsDto();
            }
        }

        public async Task<bool> HasPurchasedProductAsync(string userId, int productId)
        {
            try
            {
                return await _context.DetallesVenta
                    .AnyAsync(d => d.Venta.UsuarioId == userId && d.ProductoId == productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar compra del producto {ProductId} por usuario {UserId}", productId, userId);
                return false;
            }
        }

        public async Task<List<int>> GetPurchasedProductIdsAsync(string userId)
        {
            try
            {
                return await _context.DetallesVenta
                    .Where(d => d.Venta.UsuarioId == userId)
                    .Select(d => d.ProductoId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos comprados por usuario {UserId}", userId);
                return new List<int>();
            }
        }

        public async Task<bool> CanCommentProductAsync(string userId, int productId)
        {
            try
            {
                // Puede comentar si ha comprado el producto y no lo ha comentado ya
                var hasPurchased = await HasPurchasedProductAsync(userId, productId);
                var hasCommented = await HasCommentedProductAsync(userId, productId);

                return hasPurchased && !hasCommented;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si puede comentar producto {ProductId} usuario {UserId}", productId, userId);
                return false;
            }
        }

        public async Task<bool> HasCommentedProductAsync(string userId, int productId)
        {
            try
            {
                return await _context.Comentarios
                    .AnyAsync(c => c.UsuarioId == userId && c.ProductoId == productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar comentario del producto {ProductId} por usuario {UserId}", productId, userId);
                return false;
            }
        }
    }
}
