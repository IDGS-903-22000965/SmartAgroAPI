using SmartAgro.Models.DTOs;

namespace SmartAgro.API.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Envía credenciales de acceso a un cliente recién creado
        /// </summary>
        Task<bool> EnviarCredencialesClienteAsync(string emailDestino, string nombreCliente, string usuario, string contraseña);

        /// <summary>
        /// Envía una solicitud de cuenta al administrador
        /// </summary>
        Task<bool> EnviarSolicitudCuentaAsync(AccountRequestDto solicitud);
        Task<bool> EnviarEmailAsync(string destinatario, string asunto, string mensaje);
        Task<bool> EnviarEmailContactoAsync(string nombre, string email, string asunto, string mensaje);
        Task<bool> EnviarEmailCotizacionAsync(string email, string nombreCliente, string numeroCotizacion);
    }
}