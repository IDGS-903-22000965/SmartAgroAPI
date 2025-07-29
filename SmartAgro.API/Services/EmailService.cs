using System.Net;
using System.Net.Mail;

namespace SmartAgro.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> EnviarEmailAsync(string destinatario, string asunto, string mensaje)
        {
            try
            {
                // Obtener configuración de email
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var senderEmail = emailSettings["SenderEmail"];
                var senderName = emailSettings["SenderName"];
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

                _logger.LogInformation($"📧 Intentando enviar email a: {destinatario}");
                _logger.LogInformation($"🔧 Configuración: SMTP={smtpServer}:{smtpPort}, SSL={enableSsl}");
                _logger.LogInformation($"👤 Remitente: {senderEmail}");

                // Crear el mensaje
                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(senderEmail!, senderName);
                mailMessage.To.Add(destinatario);
                mailMessage.Subject = asunto;
                mailMessage.Body = mensaje;
                mailMessage.IsBodyHtml = false;

                // Configurar SMTP con configuración mejorada
                using var smtpClient = new SmtpClient(smtpServer, smtpPort);
                smtpClient.Credentials = new NetworkCredential(username, password);
                smtpClient.EnableSsl = enableSsl;
                smtpClient.UseDefaultCredentials = false; // Importante para Gmail
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                _logger.LogInformation($"🚀 Enviando email...");

                // Enviar email
                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation($"✅ Email enviado exitosamente a: {destinatario}");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"❌ Error SMTP al enviar email a: {destinatario}");
                _logger.LogError($"❌ SMTP Error Code: {smtpEx.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error general al enviar email a: {destinatario}");
                return false;
            }
        }

        public async Task<bool> EnviarEmailContactoAsync(string nombre, string email, string asunto, string mensaje)
        {
            var contenido = $@"
📬 NUEVO MENSAJE DE CONTACTO - SmartAgro IoT Solutions

👤 Cliente: {nombre}
📧 Email del Cliente: {email}
🏢 Empresa: (No especificada)
📱 Teléfono: (No especificado)
📋 Asunto: {asunto}

💬 Mensaje:
{mensaje}

--
📅 Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}
🌐 Enviado desde: SmartAgro IoT Solutions - Formulario de Contacto
📍 Origen: Sitio Web

⚡ ACCIÓN REQUERIDA: 
Responder al cliente ({email}) en las próximas 24 horas.

--
SmartAgro IoT Solutions
Sistema de Gestión de Contactos
            ";

            var resultado = await EnviarEmailAsync("cortezdc254@gmail.com", $"[SmartAgro] 📬 Nuevo Contacto: {asunto}", contenido);

            if (resultado)
            {
                _logger.LogInformation($"📧 Email de contacto enviado exitosamente desde: {email}");
            }
            else
            {
                _logger.LogError($"❌ Falló el envío de email de contacto desde: {email}");
            }

            return resultado;
        }

        public async Task<bool> EnviarEmailCotizacionAsync(string email, string nombreCliente, string numeroCotizacion)
        {
            var contenido = $@"
¡Hola {nombreCliente}!

Hemos recibido tu solicitud de cotización con el número: {numeroCotizacion}

📋 ¿Qué sigue ahora?
• Nuestro equipo de expertos revisará tu proyecto en detalle
• Te contactaremos en las próximas 24 horas laborables
• Recibirás una propuesta personalizada basada en tus necesidades
• Un especialista resolverá todas tus dudas

💡 Mientras tanto:
• Revisa nuestros productos y casos de éxito en nuestra página web
• Si tienes preguntas urgentes, contáctanos al WhatsApp
• Prepara cualquier información adicional que consideres relevante

🌱 En SmartAgro IoT Solutions transformamos la agricultura tradicional en agricultura inteligente, 
ayudando a optimizar recursos, aumentar rendimientos y hacer más sostenible tu producción.

¡Gracias por confiar en nosotros para llevar tu proyecto al siguiente nivel!

--
Equipo SmartAgro IoT Solutions
📧 cortezdc254@gmail.com
📱 WhatsApp: +52 477 123 4567
🌐 www.smartagro.com
📍 León de los Aldama, Guanajuato, México

💚 Juntos cultivamos el futuro de la agricultura
            ";

            var resultado = await EnviarEmailAsync(email, $"✅ Cotización #{numeroCotizacion} - SmartAgro IoT Solutions", contenido);

            if (resultado)
            {
                _logger.LogInformation($"📧 Email de cotización enviado exitosamente a cliente: {email}");
            }
            else
            {
                _logger.LogError($"❌ Falló el envío de email de cotización a: {email}");
            }

            return resultado;
        }
    }
}