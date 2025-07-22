using Microsoft.AspNetCore.Mvc;
using SmartAgro.Models.DTOs;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactoController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> EnviarContacto([FromBody] ContactoDto contactoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Aquí iría la lógica para enviar el email
                // Por ahora solo simulamos el envío

                // Log del mensaje (en producción se enviaría por email)
                Console.WriteLine($"Nuevo mensaje de contacto:");
                Console.WriteLine($"Nombre: {contactoDto.Nombre}");
                Console.WriteLine($"Email: {contactoDto.Email}");
                Console.WriteLine($"Empresa: {contactoDto.Empresa}");
                Console.WriteLine($"Teléfono: {contactoDto.Telefono}");
                Console.WriteLine($"Asunto: {contactoDto.Asunto}");
                Console.WriteLine($"Mensaje: {contactoDto.Mensaje}");

                return Ok(new
                {
                    success = true,
                    message = "Mensaje enviado exitosamente. Nos pondremos en contacto pronto."
                });
            }
            catch (Exception ex)
            {
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