using SmartAgro.Models.DTOs.Auth;

namespace SmartAgro.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> RefreshTokenAsync(string token);
        Task<bool> LogoutAsync(string userId);
    }
}