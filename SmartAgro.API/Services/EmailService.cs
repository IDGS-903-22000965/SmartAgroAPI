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
                // Por ahora solo logueamos el email (en producción se implementaría SMTP)
                _logger.LogInformation($"Email enviado a: {destinatario}");
                _logger.LogInformation($"Asunto: {asunto}");
                _logger.LogInformation($"Mensaje: {mensaje}");

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email");
                return false;
            }
        }

        public async Task<bool> EnviarEmailContactoAsync(string nombre, string email, string asunto, string mensaje)
        {
            var contenido = $@"
                Nuevo mensaje de contacto:
                Nombre: {nombre}
                Email: {email}
                Asunto: {asunto}
                Mensaje: {mensaje}
            ";

            return await EnviarEmailAsync("admin@smartagro.com", $"Contacto: {asunto}", contenido);
        }

        public async Task<bool> EnviarEmailCotizacionAsync(string email, string nombreCliente, string numeroCotizacion)
        {
            var contenido = $@"
                Estimado/a {nombreCliente},
                
                Hemos recibido su solicitud de cotización #{numeroCotizacion}.
                En breve nos pondremos en contacto con usted para proporcionarle más detalles.
                
                Gracias por confiar en SmartAgro IoT Solutions.
                
                Saludos cordiales,
                Equipo SmartAgro
            ";

            return await EnviarEmailAsync(email, $"Cotización #{numeroCotizacion} - SmartAgro IoT Solutions", contenido);
        }
    }
}