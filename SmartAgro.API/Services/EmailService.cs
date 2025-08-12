using SmartAgro.Models.DTOs;
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

        public async Task<bool> EnviarCredencialesClienteAsync(string emailDestino, string nombreCliente, string usuario, string contraseña)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
                {
                    Port = int.Parse(emailSettings["SmtpPort"]!),
                    Credentials = new NetworkCredential(
                        emailSettings["Username"],
                        emailSettings["Password"]
                    ),
                    EnableSsl = bool.Parse(emailSettings["EnableSsl"]!)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSettings["SenderEmail"]!, emailSettings["SenderName"]),
                    Subject = "🌱 Bienvenido a SmartAgro - Credenciales de Acceso",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(emailDestino);

                mailMessage.Body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; padding: 30px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; color: #2E8B57; font-weight: bold; }}
                        .credentials {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #2E8B57; }}
                        .credential-item {{ margin-bottom: 10px; }}
                        .credential-label {{ font-weight: bold; color: #2E8B57; }}
                        .credential-value {{ background-color: #e8f5e8; padding: 5px 10px; border-radius: 4px; font-family: monospace; }}
                        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>🌱 SmartAgro IoT Solutions</div>
                            <h1 style='color: #2E8B57; margin: 10px 0;'>¡Bienvenido!</h1>
                        </div>
                        
                        <p>Hola <strong>{nombreCliente}</strong>,</p>
                        <p>Tu cuenta en SmartAgro ha sido creada exitosamente. Aquí están tus credenciales de acceso:</p>
                        
                        <div class='credentials'>
                            <h3 style='color: #2E8B57; margin-top: 0;'>🔐 Credenciales de Acceso</h3>
                            <div class='credential-item'>
                                <span class='credential-label'>Usuario:</span><br>
                                <span class='credential-value'>{usuario}</span>
                            </div>
                            <div class='credential-item'>
                                <span class='credential-label'>Contraseña:</span><br>
                                <span class='credential-value'>{contraseña}</span>
                            </div>
                        </div>
                        
                        <p><strong>Enlace de acceso:</strong> <a href='http://localhost:4200/login'>Iniciar Sesión</a></p>
                        
                        <p><strong>Importante:</strong> Te recomendamos cambiar tu contraseña después del primer inicio de sesión.</p>
                        
                        <div class='footer'>
                            <p>¡Gracias por elegir SmartAgro IoT Solutions!</p>
                            <p>Si tienes problemas para acceder, contáctanos: soporte@smartagro.com</p>
                        </div>
                    </div>
                </body>
                </html>";

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"✅ Credenciales enviadas a: {emailDestino}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando credenciales a: {emailDestino}");
                return false;
            }
        }

        public async Task<bool> EnviarSolicitudCuentaAsync(AccountRequestDto solicitud)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
                {
                    Port = int.Parse(emailSettings["SmtpPort"]!),
                    Credentials = new NetworkCredential(
                        emailSettings["Username"],
                        emailSettings["Password"]
                    ),
                    EnableSsl = bool.Parse(emailSettings["EnableSsl"]!)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSettings["SenderEmail"]!, emailSettings["SenderName"]),
                    Subject = $"🌱 Nueva Solicitud de Cuenta - {solicitud.Nombre} {solicitud.Apellidos}",
                    IsBodyHtml = true
                };

                // ✅ CAMBIO: Usar el AdminEmail de configuración
                var adminEmail = emailSettings["AdminEmail"] ?? emailSettings["SenderEmail"];
                mailMessage.To.Add(adminEmail!);

                _logger.LogInformation($"📧 Enviando solicitud de cuenta al admin: {adminEmail}");

                mailMessage.Body = $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; padding: 30px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                .header {{ text-align: center; margin-bottom: 30px; }}
                .logo {{ font-size: 24px; color: #2E8B57; font-weight: bold; }}
                .content {{ line-height: 1.6; color: #333; }}
                .info-section {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                .info-row {{ display: flex; margin-bottom: 10px; }}
                .info-label {{ font-weight: bold; min-width: 120px; color: #2E8B57; }}
                .info-value {{ flex: 1; }}
                .message-box {{ background-color: #e8f5e8; padding: 15px; border-left: 4px solid #2E8B57; margin: 20px 0; }}
                .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; color: #666; }}
                .btn {{ display: inline-block; background-color: #2E8B57; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 10px 5px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <div class='logo'>🌱 SmartAgro IoT Solutions</div>
                    <h1 style='color: #2E8B57; margin: 10px 0;'>Nueva Solicitud de Cuenta</h1>
                </div>
                
                <div class='content'>
                    <p>Se ha recibido una nueva solicitud de cuenta de cliente con los siguientes datos:</p>
                    
                    <div class='info-section'>
                        <h3 style='color: #2E8B57; margin-top: 0;'>📋 Información del Solicitante</h3>
                        <div class='info-row'>
                            <span class='info-label'>Nombre:</span>
                            <span class='info-value'>{solicitud.Nombre} {solicitud.Apellidos}</span>
                        </div>
                        <div class='info-row'>
                            <span class='info-label'>Email:</span>
                            <span class='info-value'>{solicitud.Email}</span>
                        </div>
                        
                        <div class='info-row'>
                            <span class='info-label'>Fecha:</span>
                            <span class='info-value'>{solicitud.FechaSolicitud:dd/MM/yyyy HH:mm}</span>
                        </div>
                    </div>
                    
                    <div class='message-box'>
                        <h4 style='color: #2E8B57; margin-top: 0;'>💬 Mensaje del solicitante:</h4>
                        <p>{solicitud.Mensaje}</p>
                    </div>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <p><strong>📋 Acciones recomendadas:</strong></p>
                        <ol style='text-align: left; max-width: 400px; margin: 0 auto;'>
                            <li>Revisar la información del solicitante</li>
                            <li>Contactar al cliente para confirmar detalles</li>
                            <li>Crear la cuenta desde el panel de administración</li>
                            <li>Las credenciales se enviarán automáticamente por email</li>
                        </ol>
                        
                        <div style='margin-top: 20px;'>
                            <a href='http://localhost:4200/admin/usuarios' class='btn'>🔗 Crear Cuenta en Panel Admin</a>
                        </div>
                    </div>
                </div>
                
                <div class='footer'>
                    <p>Este email fue generado automáticamente por el sistema SmartAgro IoT Solutions.</p>
                    <p><strong>Enviado a:</strong> {adminEmail}</p>
                    <p>Para crear la cuenta, inicia sesión en el panel de administración.</p>
                </div>
            </div>
        </body>
        </html>";

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"✅ Email de solicitud enviado para: {solicitud.Email} al admin: {adminEmail}");

                // También enviar confirmación al solicitante
                await EnviarConfirmacionSolicitudAsync(solicitud);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando email de solicitud: {solicitud.Email}");
                return false;
            }
        }

        // ✅ MÉTODO FALTANTE 1
        public async Task<bool> EnviarEmailAsync(string destinatario, string asunto, string mensaje)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
                {
                    Port = int.Parse(emailSettings["SmtpPort"]!),
                    Credentials = new NetworkCredential(
                        emailSettings["Username"],
                        emailSettings["Password"]
                    ),
                    EnableSsl = bool.Parse(emailSettings["EnableSsl"]!)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSettings["SenderEmail"]!, emailSettings["SenderName"]),
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(destinatario);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"✅ Email genérico enviado a: {destinatario}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando email genérico a: {destinatario}");
                return false;
            }
        }

        // ✅ MÉTODO FALTANTE 2
        public async Task<bool> EnviarEmailContactoAsync(string nombre, string email, string asunto, string mensaje)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
                {
                    Port = int.Parse(emailSettings["SmtpPort"]!),
                    Credentials = new NetworkCredential(
                        emailSettings["Username"],
                        emailSettings["Password"]
                    ),
                    EnableSsl = bool.Parse(emailSettings["EnableSsl"]!)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSettings["SenderEmail"]!, emailSettings["SenderName"]),
                    Subject = $"📞 Contacto desde Web - {asunto}",
                    IsBodyHtml = true
                };

                var adminEmail = emailSettings["AdminEmail"] ?? emailSettings["SenderEmail"];
                mailMessage.To.Add(adminEmail!);

                mailMessage.Body = $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; padding: 30px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                .header {{ text-align: center; margin-bottom: 30px; }}
                .logo {{ font-size: 24px; color: #2E8B57; font-weight: bold; }}
                .info-box {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                .message-box {{ background-color: #e8f5e8; padding: 15px; border-left: 4px solid #2E8B57; margin: 20px 0; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <div class='logo'>🌱 SmartAgro IoT Solutions</div>
                    <h1 style='color: #2E8B57; margin: 10px 0;'>Nuevo Mensaje de Contacto</h1>
                </div>
                
                <div class='info-box'>
                    <h3 style='color: #2E8B57; margin-top: 0;'>📋 Información del Contacto</h3>
                    <p><strong>Nombre:</strong> {nombre}</p>
                    <p><strong>Email:</strong> {email}</p>
                    <p><strong>Asunto:</strong> {asunto}</p>
                    <p><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                </div>
                
                <div class='message-box'>
                    <h4 style='color: #2E8B57; margin-top: 0;'>💬 Mensaje:</h4>
                    <p>{mensaje}</p>
                </div>
            </div>
        </body>
        </html>";

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"✅ Email de contacto enviado desde: {email} al admin: {adminEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando email de contacto desde: {email}");
                return false;
            }
        }

        // ✅ MÉTODO FALTANTE 3
        public async Task<bool> EnviarEmailCotizacionAsync(string email, string nombreCliente, string numeroCotizacion)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
                {
                    Port = int.Parse(emailSettings["SmtpPort"]!),
                    Credentials = new NetworkCredential(
                        emailSettings["Username"],
                        emailSettings["Password"]
                    ),
                    EnableSsl = bool.Parse(emailSettings["EnableSsl"]!)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSettings["SenderEmail"]!, emailSettings["SenderName"]),
                    Subject = $"🌱 Cotización SmartAgro - #{numeroCotizacion}",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                mailMessage.Body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; padding: 30px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; color: #2E8B57; font-weight: bold; }}
                        .cotizacion-box {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #2E8B57; }}
                        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>🌱 SmartAgro IoT Solutions</div>
                            <h1 style='color: #2E8B57; margin: 10px 0;'>Tu Cotización está Lista</h1>
                        </div>
                        
                        <p>Hola <strong>{nombreCliente}</strong>,</p>
                        <p>Hemos preparado tu cotización personalizada para el sistema de riego inteligente.</p>
                        
                        <div class='cotizacion-box'>
                            <h3 style='color: #2E8B57; margin-top: 0;'>📋 Cotización #{numeroCotizacion}</h3>
                            <p>Tu cotización ha sido generada y está disponible para revisión.</p>
                            <p><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy}</p>
                        </div>
                        
                        <p>Nuestro equipo se pondrá en contacto contigo para revisar los detalles y resolver cualquier duda.</p>
                        
                        <div class='footer'>
                            <p>¡Gracias por elegir SmartAgro IoT Solutions!</p>
                            <p>Para más información: soporte@smartagro.com | +52 (477) 123-4567</p>
                        </div>
                    </div>
                </body>
                </html>";

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"✅ Email de cotización enviado a: {email} - #{numeroCotizacion}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando cotización a: {email} - #{numeroCotizacion}");
                return false;
            }
        }

        // ✅ MÉTODO PRIVADO (ya existía)
        private async Task<bool> EnviarConfirmacionSolicitudAsync(AccountRequestDto solicitud)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
                {
                    Port = int.Parse(emailSettings["SmtpPort"]!),
                    Credentials = new NetworkCredential(
                        emailSettings["Username"],
                        emailSettings["Password"]
                    ),
                    EnableSsl = bool.Parse(emailSettings["EnableSsl"]!)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSettings["SenderEmail"]!, emailSettings["SenderName"]),
                    Subject = "🌱 Solicitud de cuenta recibida - SmartAgro",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(solicitud.Email);

                mailMessage.Body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; padding: 30px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; color: #2E8B57; font-weight: bold; }}
                        .success-box {{ background-color: #e8f5e8; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0; border-left: 4px solid #2E8B57; }}
                        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; color: #666; font-size: 14px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>🌱 SmartAgro IoT Solutions</div>
                            <h1 style='color: #2E8B57; margin: 10px 0;'>¡Solicitud Recibida!</h1>
                        </div>
                        
                        <div class='success-box'>
                            <h2 style='color: #2E8B57; margin-top: 0;'>✅ Tu solicitud ha sido enviada exitosamente</h2>
                            <p>Hola <strong>{solicitud.Nombre}</strong>, hemos recibido tu solicitud para crear una cuenta en SmartAgro.</p>
                        </div>
                        
                        <p>Nos pondremos en contacto contigo en las próximas 24 horas.</p>
                        
                        <div class='footer'>
                            <p>Gracias por tu interés en SmartAgro IoT Solutions.</p>
                        </div>
                    </div>
                </body>
                </html>";

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"✅ Email de confirmación enviado a: {solicitud.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando confirmación a: {solicitud.Email}");
                return false;
            }
        }
    }
}