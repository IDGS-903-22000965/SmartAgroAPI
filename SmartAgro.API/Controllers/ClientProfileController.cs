// SmartAgro.API/Controllers/ClientProfileController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Models.DTOs.Auth;
using SmartAgro.Models.DTOs.Users;
using SmartAgro.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/client/profile")]
    [Authorize(Roles = "Cliente")] // Solo clientes pueden acceder
    public class ClientProfileController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly ILogger<ClientProfileController> _logger;

        public ClientProfileController(
            UserManager<Usuario> userManager,
            ILogger<ClientProfileController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el perfil completo del cliente actual
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ClientProfileDto>> GetClientProfile()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(currentUserId);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado" });

                var roles = await _userManager.GetRolesAsync(user);

                var profile = new ClientProfileDto
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Apellidos = user.Apellidos,
                    Email = user.Email!,
                    Telefono = user.Telefono,
                    Direccion = user.Direccion,
                    FechaRegistro = user.FechaRegistro,
                    Activo = user.Activo,
                    Roles = roles.ToList()
                };

                return Ok(new { success = true, data = profile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil del cliente");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza los datos básicos del perfil del cliente
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateClientProfile([FromBody] UpdateClientProfileDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(currentUserId);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado" });

                // Verificar si el email ya está en uso por otro usuario
                var existingUser = await _userManager.FindByEmailAsync(updateDto.Email);
                if (existingUser != null && existingUser.Id != currentUserId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El email ya está en uso por otro usuario"
                    });
                }

                // Actualizar datos
                user.Nombre = updateDto.Nombre;
                user.Apellidos = updateDto.Apellidos;
                user.Email = updateDto.Email;
                user.UserName = updateDto.Email; // UserName debe coincidir con Email
                user.Telefono = updateDto.Telefono;
                user.Direccion = updateDto.Direccion;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Error al actualizar perfil",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                _logger.LogInformation($"✅ Perfil actualizado para cliente: {user.Email}");

                return Ok(new
                {
                    success = true,
                    message = "Perfil actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar perfil del cliente");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Cambia la contraseña del cliente
        /// </summary>
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(currentUserId);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado" });

                var result = await _userManager.ChangePasswordAsync(
                    user,
                    changePasswordDto.CurrentPassword,
                    changePasswordDto.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Error al cambiar contraseña",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                _logger.LogInformation($"✅ Contraseña cambiada para cliente: {user.Email}");

                return Ok(new
                {
                    success = true,
                    message = "Contraseña cambiada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña del cliente");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Obtiene estadísticas básicas del cliente (compras, comentarios, etc.)
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ClientStatsDto>> GetClientStats()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                // Estas estadísticas se implementarán en otros controladores
                var stats = new ClientStatsDto
                {
                    TotalCompras = 0,
                    MontoTotalGastado = 0,
                    ComentariosRealizados = 0,
                    UltimaCompra = null
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del cliente");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }
    }
}

// DTOs específicos para clientes
namespace SmartAgro.Models.DTOs.Users
{
    public class ClientProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string NombreCompleto => $"{Nombre} {Apellidos}";
    }

    public class UpdateClientProfileDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? Telefono { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string? Direccion { get; set; }
    }

    public class ClientStatsDto
    {
        public int TotalCompras { get; set; }
        public decimal MontoTotalGastado { get; set; }
        public int ComentariosRealizados { get; set; }
        public DateTime? UltimaCompra { get; set; }
    }
}