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

                // Agregar detalles de la cotización
                await AgregarDetallesCotizacionAsync(cotizacion, request);

                // Enviar email de confirmación
                try
                {
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
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, $"❌ Error al enviar email a: {cotizacion.EmailCliente}");
                    // No fallar la cotización si falla el email
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
            try
            {
                _logger.LogInformation("🔄 Calculando costo para área: {Area}m², cultivo: {Cultivo}",
                    request.AreaCultivo, request.TipoCultivo);

                // ✅ VERIFICAR SI EXISTE AL MENOS UN PRODUCTO EN LA BASE DE DATOS
                var productos = await _context.Productos.Where(p => p.Activo).ToListAsync();
                if (!productos.Any())
                {
                    _logger.LogWarning("⚠️ No hay productos activos en la base de datos");
                    // Usar precio base por defecto si no hay productos
                    return CalcularCostoSinProductos(request);
                }

                // Obtener el primer producto activo como producto principal
                var producto = productos.First();
                _logger.LogInformation("📦 Usando producto: {Nombre} - Precio: ${Precio}",
                    producto.Nombre, producto.PrecioVenta);

                // Calcular cantidad de sistemas necesarios basado en el área
                var cantidadSistemas = CalcularCantidadSistemas(request.AreaCultivo);
                _logger.LogInformation("🔢 Cantidad de sistemas calculada: {Cantidad}", cantidadSistemas);

                // Costo base
                var costoBase = cantidadSistemas * producto.PrecioVenta;

                // Factores de ajuste
                decimal factorTipoCultivo = ObtenerFactorTipoCultivo(request.TipoCultivo);
                decimal factorTipoSuelo = ObtenerFactorTipoSuelo(request.TipoSuelo);

                // Costos adicionales
                decimal costoAdicionalAgua = !request.FuenteAguaDisponible ? 800.00m : 0.00m;
                decimal costoAdicionalEnergia = !request.EnergiaElectricaDisponible ? 1200.00m : 0.00m;

                var costoTotal = costoBase * factorTipoCultivo * factorTipoSuelo + costoAdicionalAgua + costoAdicionalEnergia;

                _logger.LogInformation("💰 Desglose del costo - Base: ${CostoBase}, Factor cultivo: {FactorCultivo}, Factor suelo: {FactorSuelo}, Agua: ${CostoAgua}, Energía: ${CostoEnergia}, Total: ${CostoTotal}",
                    costoBase, factorTipoCultivo, factorTipoSuelo, costoAdicionalAgua, costoAdicionalEnergia, costoTotal);

                return Math.Round(costoTotal, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al calcular costo de cotización");
                throw new Exception($"Error en el cálculo de costo: {ex.Message}", ex);
            }
        }

        private decimal CalcularCostoSinProductos(CotizacionRequestDto request)
        {
            _logger.LogInformation("🔧 Calculando costo sin productos en BD, usando precios base");

            // Precio base por m² según tipo de cultivo
            decimal precioPorM2 = request.TipoCultivo?.ToLower() switch
            {
                "hortalizas" => 150.00m,
                "frutales" => 200.00m,
                "cereales" => 120.00m,
                "flores" => 180.00m,
                _ => 150.00m
            };

            var costoBase = request.AreaCultivo * precioPorM2;

            // Aplicar factores
            decimal factorTipoSuelo = ObtenerFactorTipoSuelo(request.TipoSuelo);
            decimal costoAdicionalAgua = !request.FuenteAguaDisponible ? 800.00m : 0.00m;
            decimal costoAdicionalEnergia = !request.EnergiaElectricaDisponible ? 1200.00m : 0.00m;

            return Math.Round(costoBase * factorTipoSuelo + costoAdicionalAgua + costoAdicionalEnergia, 2);
        }

        private decimal ObtenerFactorTipoCultivo(string? tipoCultivo)
        {
            return tipoCultivo?.ToLower() switch
            {
                "hortalizas" => 1.0m,
                "frutales" => 1.2m,
                "cereales" => 0.9m,
                "flores" => 1.1m,
                _ => 1.0m
            };
        }

        private decimal ObtenerFactorTipoSuelo(string? tipoSuelo)
        {
            return tipoSuelo?.ToLower() switch
            {
                "arcilloso" => 1.1m,
                "arenoso" => 1.0m,
                "limoso" => 0.95m,
                _ => 1.0m
            };
        }

        private int CalcularCantidadSistemas(decimal areaCultivo)
        {
            // Cada sistema puede cubrir hasta 100m²
            return (int)Math.Ceiling(areaCultivo / 100m);
        }

        private async Task AgregarDetallesCotizacionAsync(Cotizacion cotizacion, CotizacionRequestDto request)
        {
            try
            {
                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Activo);
                if (producto != null)
                {
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

                    _logger.LogInformation("✅ Detalle de cotización guardado");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al agregar detalles de cotización");
                // No fallar toda la cotización por este error
            }
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