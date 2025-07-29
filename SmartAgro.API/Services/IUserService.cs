// SmartAgro.API/Services/IUserService.cs
using SmartAgro.Models.DTOs.Users;
using SmartAgro.Models.DTOs; // Agregar este using

namespace SmartAgro.API.Services
{
    public interface IUserService
    {
        Task<PaginatedUsersDto> GetUsersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? roleFilter = null, bool? isActive = null);
        Task<UserListDto?> GetUserByIdAsync(string userId);
        Task<ServiceResult> CreateUserAsync(CreateUserDto createUserDto);        
        Task<ServiceResult> UpdateUserAsync(string userId, UpdateUserDto updateUserDto);  
        Task<ServiceResult> DeleteUserAsync(string userId);                      
        Task<ServiceResult> ToggleUserStatusAsync(string userId);               
        Task<ServiceResult> ResetPasswordAsync(string userId, ResetPasswordDto resetPasswordDto); 
        Task<UserStatsDto> GetUserStatsAsync();
        Task<List<string>> GetAvailableRolesAsync();
    }
}