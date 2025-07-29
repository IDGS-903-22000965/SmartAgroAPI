using Microsoft.AspNetCore.Mvc;
using SmartAgro.Models.DTOs;
using SmartAgro.API.Services;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactoController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<ContactoController> _logger;

        public ContactoController(
            IEmailService emailService,
            ILogger<ContactoController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> EnviarContacto([FromBody] ContactoDto contactoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation($"🔄 Procesando mensaje de contacto de: {contactoDto.Email}");

                var emailEnviado = await _emailService.EnviarEmailContactoAsync(
                    contactoDto.Nombre,
                    contactoDto.Email,
                    contactoDto.Asunto,
                    contactoDto.Mensaje
                );

                if (emailEnviado)
                {
                    _logger.LogInformation($"✅ Email de contacto enviado exitosamente desde: {contactoDto.Email}");

                    return Ok(new
                    {
                        success = true,
                        message = "Mensaje enviado exitosamente. Nos pondremos en contacto pronto."
                    });
                }
                else
                {
                    _logger.LogWarning($"⚠️ No se pudo enviar el email de contacto desde: {contactoDto.Email}");

                    return Ok(new
                    {
                        success = true,
                        message = "Mensaje recibido. Nos pondremos en contacto pronto."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al procesar mensaje de contacto de: {contactoDto.Email}");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al enviar el mensaje",
                    error = ex.Message
                });
            }
        }
    }
}