using SmartAgro.Models.DTOs.Auth;

namespace SmartAgro.API.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Autentica un usuario con email y contraseña
        /// </summary>
        /// <param name="loginDto">Datos de login</param>
        /// <returns>Respuesta de autenticación con token si es exitoso</returns>
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// Refresca un token de acceso expirado
        /// </summary>
        /// <param name="token">Token a refrescar</param>
        /// <returns>Nuevo token si es válido</returns>
        Task<AuthResponseDto> RefreshTokenAsync(string token);

        /// <summary>
        /// Cierra la sesión de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>True si fue exitoso</returns>
        Task<bool> LogoutAsync(string userId);

        // ❌ REMOVIDO: RegisterAsync ya no forma parte de la interfaz
        // El registro de clientes se maneja a través del IUserService
    }
}