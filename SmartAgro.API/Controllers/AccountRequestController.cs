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
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 100 caracteres")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Teléfono inválido")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe tener 10 dígitos")]
        public string? Telefono { get; set; }

        [StringLength(200, ErrorMessage = "El nombre de la empresa no puede exceder 200 caracteres")]
        public string? Empresa { get; set; }

        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 500 caracteres")]
        public string Mensaje { get; set; } = string.Empty;

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    }
}