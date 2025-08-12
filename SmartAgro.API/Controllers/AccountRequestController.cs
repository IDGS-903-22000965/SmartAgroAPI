using Microsoft.AspNetCore.Mvc;
using SmartAgro.API.Services;
using SmartAgro.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/account-request")]  // ✅ Cambiado para que coincida con el frontend
    public class AccountRequestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountRequestController> _logger;

        public AccountRequestController(
            IEmailService emailService,
            ILogger<AccountRequestController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Envía una solicitud de cuenta al administrador
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitAccountRequest([FromBody] AccountRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Datos inválidos en la solicitud"
                });
            }

            try
            {
                // Enviar email al administrador
                var emailSent = await _emailService.EnviarSolicitudCuentaAsync(request);

                if (emailSent)
                {
                    _logger.LogInformation($"✅ Solicitud de cuenta enviada para: {request.Email}");

                    return Ok(new
                    {
                        success = true,
                        message = "Solicitud enviada exitosamente. Te contactaremos pronto."
                    });
                }
                else
                {
                    _logger.LogWarning($"⚠️ No se pudo enviar solicitud de cuenta para: {request.Email}");

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Error al enviar la solicitud. Intenta nuevamente o contáctanos por teléfono."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error procesando solicitud de cuenta: {request.Email}");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor. Contáctanos por teléfono si el problema persiste."
                });
            }
        }
    }
}


namespace SmartAgro.Models.DTOs
{
    public class AccountRequestDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100, MinimumLength = 2)]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [RegularExpression(@"^\d{10}$")]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Empresa { get; set; }

        [StringLength(500, MinimumLength = 10)]
        public string? Mensaje { get; set; } // ✅ Ya no es requerido

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    }

}