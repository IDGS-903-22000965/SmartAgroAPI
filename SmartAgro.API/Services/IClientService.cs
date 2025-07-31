using SmartAgro.Models.DTOs.Users;

namespace SmartAgro.API.Services
{
    public interface IClientService
    {
        Task<ClientStatsDto> GetClientStatsAsync(string userId);
        Task<bool> HasPurchasedProductAsync(string userId, int productId);
        Task<List<int>> GetPurchasedProductIdsAsync(string userId);
        Task<bool> CanCommentProductAsync(string userId, int productId);
        Task<bool> HasCommentedProductAsync(string userId, int productId);
    }
}
