using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAgro.API.Services;
using SmartAgro.Models.DTOs.Auth;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Iniciar sesión de usuario
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido en login para: {Email}", loginDto.Email);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Intento de login para: {Email}", loginDto.Email);

            var result = await _authService.LoginAsync(loginDto);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Login fallido para: {Email} - {Message}", loginDto.Email, result.Message);
                return Unauthorized(result);
            }

            _logger.LogInformation("Login exitoso para: {Email}", loginDto.Email);
            return Ok(result);
        }

        /// <summary>
        /// Refrescar token de acceso
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Token es requerido"
                });
            }

            var result = await _authService.RefreshTokenAsync(token);

            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Cerrar sesión
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // En este caso, el logout se maneja principalmente en el frontend
                // eliminando el token del localStorage, pero podemos registrar la acción
                _logger.LogInformation("Usuario cerró sesión");

                return Ok(new
                {
                    success = true,
                    message = "Sesión cerrada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante logout");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        
    }
}