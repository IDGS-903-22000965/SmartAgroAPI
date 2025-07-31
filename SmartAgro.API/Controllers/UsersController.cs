using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAgro.API.Services;
using SmartAgro.Models.DTOs.Users;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Solo administradores pueden gestionar usuarios
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una lista paginada de usuarios con filtros opcionales
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedUsersDto>> GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? roleFilter = null,
            [FromQuery] bool? isActive = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _userService.GetUsersAsync(pageNumber, pageSize, searchTerm, roleFilter, isActive);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un usuario específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserListDto>> GetUser(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            return Ok(user);
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            // ✅ LOGS DE DEBUG DETALLADOS
            _logger.LogInformation($"🔥 RECIBIDA solicitud de creación de usuario");
            _logger.LogInformation($"🔧 DEBUG - Nombre: '{createUserDto?.Nombre ?? "NULL"}'");
            _logger.LogInformation($"🔧 DEBUG - Apellidos: '{createUserDto?.Apellidos ?? "NULL"}'");
            _logger.LogInformation($"🔧 DEBUG - Email: '{createUserDto?.Email ?? "NULL"}'");
            _logger.LogInformation($"🔧 DEBUG - Password Length: {createUserDto?.Password?.Length ?? 0}");
            _logger.LogInformation($"🔧 DEBUG - Telefono: '{createUserDto?.Telefono ?? "NULL"}'");
            _logger.LogInformation($"🔧 DEBUG - Direccion: '{createUserDto?.Direccion ?? "NULL"}'");
            _logger.LogInformation($"🔧 DEBUG - Rol: '{createUserDto?.Rol ?? "NULL"}'");
            _logger.LogInformation($"🔧 DEBUG - Activo: {createUserDto?.Activo}");
            _logger.LogInformation($"🔧 DEBUG - ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"⚠️ ModelState inválido para usuario: {createUserDto?.Email ?? "NULL"}");

                // ✅ LOGS DETALLADOS DE ERRORES DE VALIDACIÓN
                foreach (var error in ModelState)
                {
                    foreach (var errorMessage in error.Value.Errors)
                    {
                        _logger.LogWarning($"❌ Campo '{error.Key}': {errorMessage.ErrorMessage}");
                    }
                }

                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation($"📝 Llamando a UserService.CreateUserAsync para: {createUserDto.Email}");
                var result = await _userService.CreateUserAsync(createUserDto);

                if (!result.Success)
                {
                    _logger.LogError($"❌ Error desde UserService para {createUserDto.Email}: {result.Message}");
                    return BadRequest(new { message = result.Message });
                }

                _logger.LogInformation($"✅ Usuario creado exitosamente: {createUserDto.Email}");
                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"💥 Excepción no controlada al crear usuario: {createUserDto?.Email ?? "NULL"}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.UpdateUserAsync(id, updateUserDto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteUserAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Cambia el estado activo/inactivo de un usuario
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var result = await _userService.ToggleUserStatusAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Restablece la contraseña de un usuario (solo admin)
        /// </summary>
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.ResetPasswordAsync(id, resetPasswordDto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Obtiene estadísticas de usuarios
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<UserStatsDto>> GetUserStats()
        {
            var stats = await _userService.GetUserStatsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Obtiene la lista de roles disponibles
        /// </summary>
        [HttpGet("roles")]
        public async Task<ActionResult<List<string>>> GetAvailableRoles()
        {
            var roles = await _userService.GetAvailableRolesAsync();
            return Ok(roles);
        }
    }
}