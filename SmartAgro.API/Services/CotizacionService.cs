using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.DTOs;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public class CotizacionService : ICotizacionService
    {
        private readonly SmartAgroDbContext _context;
        private readonly IEmailService _emailService; 
        private readonly ILogger<CotizacionService> _logger; 

        public CotizacionService(
            SmartAgroDbContext context,
            IEmailService emailService, 
            ILogger<CotizacionService> logger) 
        {
            _context = context;
            _emailService = emailService;
            _logger = logger; 
        }

        public async Task<Cotizacion> CrearCotizacionAsync(CotizacionRequestDto request)
        {
            try
            {
                _logger.LogInformation($"🔄 Iniciando creación de cotización para: {request.EmailCliente}");

                // Calcular el costo basado en el área y tipo de cultivo
                var costoCotizacion = await CalcularCostoCotizacionAsync(request);
                var subtotal = costoCotizacion;
                var impuestos = subtotal * 0.16m; // IVA 16%
                var total = subtotal + impuestos;

                var cotizacion = new Cotizacion
                {
                    NumeroCotizacion = await GenerarNumeroCotizacionAsync(),
                    NombreCliente = request.NombreCliente,
                    EmailCliente = request.EmailCliente,
                    TelefonoCliente = request.TelefonoCliente,
                    DireccionInstalacion = request.DireccionInstalacion,
                    AreaCultivo = request.AreaCultivo,
                    TipoCultivo = request.TipoCultivo,
                    TipoSuelo = request.TipoSuelo,
                    FuenteAguaDisponible = request.FuenteAguaDisponible,
                    EnergiaElectricaDisponible = request.EnergiaElectricaDisponible,
                    RequierimientosEspeciales = request.RequierimientosEspeciales,
                    Subtotal = subtotal,
                    PorcentajeImpuesto = 16,
                    Impuestos = impuestos,
                    Total = total,
                    FechaVencimiento = DateTime.Now.AddDays(30),
                    Estado = "Pendiente"
                };

                _context.Cotizaciones.Add(cotizacion);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Cotización guardada con número: {cotizacion.NumeroCotizacion}");

                // Agregar detalles de la cotización (producto principal)
                var producto = await _context.Productos.FirstAsync(p => p.Id == 1); // Sistema principal
                var cantidad = CalcularCantidadSistemas(request.AreaCultivo);

                var detalle = new DetalleCotizacion
                {
                    CotizacionId = cotizacion.Id,
                    ProductoId = producto.Id,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.PrecioVenta,
                    Subtotal = cantidad * producto.PrecioVenta,
                    Descripcion = $"Sistema de Riego Automático Inteligente para {request.AreaCultivo}m² de {request.TipoCultivo}"
                };

                _context.DetallesCotizacion.Add(detalle);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Detalle de cotización guardado");

                // ¡AQUÍ ES DONDE FALTABA EL ENVÍO DE EMAIL!
                _logger.LogInformation($"📧 Enviando email de confirmación a: {cotizacion.EmailCliente}");

                var emailEnviado = await _emailService.EnviarEmailCotizacionAsync(
                    cotizacion.EmailCliente,
                    cotizacion.NombreCliente,
                    cotizacion.NumeroCotizacion
                );

                if (emailEnviado)
                {
                    _logger.LogInformation($"✅ Email enviado exitosamente a: {cotizacion.EmailCliente}");
                }
                else
                {
                    _logger.LogWarning($"⚠️ No se pudo enviar el email a: {cotizacion.EmailCliente}");
                }

                return cotizacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al crear cotización para: {request.EmailCliente}");
                throw;
            }
        }

        public async Task<List<Cotizacion>> ObtenerCotizacionesAsync()
        {
            return await _context.Cotizaciones
                .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
                .OrderByDescending(c => c.FechaCotizacion)
                .ToListAsync();
        }

        public async Task<Cotizacion?> ObtenerCotizacionPorIdAsync(int id)
        {
            return await _context.Cotizaciones
                .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Cotizacion>> ObtenerCotizacionesPorUsuarioAsync(string usuarioId)
        {
            return await _context.Cotizaciones
                .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
                .Where(c => c.UsuarioId == usuarioId)
                .OrderByDescending(c => c.FechaCotizacion)
                .ToListAsync();
        }

        public async Task<bool> ActualizarEstadoCotizacionAsync(int id, string estado)
        {
            var cotizacion = await _context.Cotizaciones.FindAsync(id);
            if (cotizacion == null) return false;

            cotizacion.Estado = estado;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalcularCostoCotizacionAsync(CotizacionRequestDto request)
        {
            // Obtener el producto principal
            var producto = await _context.Productos.FirstAsync(p => p.Id == 1);

            // Calcular cantidad de sistemas necesarios basado en el área
            var cantidadSistemas = CalcularCantidadSistemas(request.AreaCultivo);

            // Costo base
            var costoBase = cantidadSistemas * producto.PrecioVenta;

            // Factores de ajuste
            decimal factorTipoCultivo = request.TipoCultivo.ToLower() switch
            {
                "hortalizas" => 1.0m,
                "frutales" => 1.2m,
                "cereales" => 0.9m,
                "flores" => 1.1m,
                _ => 1.0m
            };

            decimal factorTipoSuelo = request.TipoSuelo.ToLower() switch
            {
                "arcilloso" => 1.1m,
                "arenoso" => 1.0m,
                "limoso" => 0.95m,
                _ => 1.0m
            };

            // Costos adicionales
            decimal costoAdicionalAgua = !request.FuenteAguaDisponible ? 800.00m : 0.00m;
            decimal costoAdicionalEnergia = !request.EnergiaElectricaDisponible ? 1200.00m : 0.00m;

            var costoTotal = costoBase * factorTipoCultivo * factorTipoSuelo + costoAdicionalAgua + costoAdicionalEnergia;

            return Math.Round(costoTotal, 2);
        }

        private int CalcularCantidadSistemas(decimal areaCultivo)
        {
            // Cada sistema puede cubrir hasta 100m²
            return (int)Math.Ceiling(areaCultivo / 100m);
        }

        private async Task<string> GenerarNumeroCotizacionAsync()
        {
            var fecha = DateTime.Now;
            var consecutivo = await _context.Cotizaciones
                .Where(c => c.FechaCotizacion.Year == fecha.Year &&
                           c.FechaCotizacion.Month == fecha.Month)
                .CountAsync() + 1;

            return $"COT-{fecha:yyyyMM}-{consecutivo:D4}";
        }
    }
}