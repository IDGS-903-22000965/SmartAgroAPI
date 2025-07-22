namespace SmartAgro.API.Services
{
    public interface IEmailService
    {
        Task<bool> EnviarEmailAsync(string destinatario, string asunto, string mensaje);
        Task<bool> EnviarEmailContactoAsync(string nombre, string email, string asunto, string mensaje);
        Task<bool> EnviarEmailCotizacionAsync(string email, string nombreCliente, string numeroCotizacion);
    }
}