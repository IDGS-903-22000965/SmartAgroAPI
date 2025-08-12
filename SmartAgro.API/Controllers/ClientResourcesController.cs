// SmartAgro.API/Controllers/ClientResourcesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.DTOs;
using System.Security.Claims;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/client/resources")]
    [Authorize(Roles = "Cliente")]
    public class ClientResourcesController : ControllerBase
    {
        private readonly SmartAgroDbContext _context;
        private readonly ILogger<ClientResourcesController> _logger;

        public ClientResourcesController(
            SmartAgroDbContext context,
            ILogger<ClientResourcesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("client/resources")]
        [Authorize]
        public async Task<IActionResult> GetClientProductResources()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var productosConVenta = await _context.Ventas
                .Where(v => v.UsuarioId == userId)
                .SelectMany(v => v.Detalles)
                .Select(d => d.ProductoId)
                .Distinct()
                .ToListAsync();

            var recursos = await _context.ProductoDocumentos
                .Where(d => productosConVenta.Contains(d.ProductoId) && d.EsVisibleParaCliente)
                .Select(d => new
                {
                    ProductoId = d.ProductoId,
                    Titulo = d.Titulo,
                    Tipo = d.Tipo,
                    Url = d.Url
                })
                .ToListAsync();

            return Ok(new { success = true, data = recursos });
        }

        /// <summary>
        /// Obtiene todos los recursos disponibles para los productos comprados por el cliente
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ClientProductResourceDto>>> GetMyProductResources()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                // Obtener productos únicos comprados por el cliente
                var ownedProducts = await _context.DetallesVenta
                    .Where(d => d.Venta.UsuarioId == currentUserId)
                    .Include(d => d.Producto)
                    .Include(d => d.Venta)
                    .GroupBy(d => d.ProductoId)
                    .ToListAsync();

                var result = new List<ClientProductResourceDto>();

                foreach (var group in ownedProducts)
                {
                    var firstItem = group.First();
                    var productDto = new ClientProductResourceDto
                    {
                        ProductoId = group.Key,
                        NombreProducto = firstItem.Producto.Nombre,
                        DescripcionProducto = firstItem.Producto.Descripcion,
                        DescripcionDetallada = firstItem.Producto.DescripcionDetallada,
                        ImagenPrincipal = firstItem.Producto.ImagenPrincipal,
                        ImagenesSecundarias = DeserializeStringList(firstItem.Producto.ImagenesSecundarias),
                        VideoDemo = firstItem.Producto.VideoDemo,
                        Caracteristicas = DeserializeStringList(firstItem.Producto.Caracteristicas),
                        Beneficios = DeserializeStringList(firstItem.Producto.Beneficios),

                        // Información de compra
                        PrimeraCompra = group.Min(d => d.Venta.FechaVenta),
                        UltimaCompra = group.Max(d => d.Venta.FechaVenta),
                        TotalComprado = group.Sum(d => d.Cantidad),

                        // Recursos y documentación - ACTUALIZADO PARA USAR DATOS REALES
                        ManualesDisponibles = await GetManualesParaProducto(group.Key),
                        GuiasMantenimiento = await GetGuiasMantenimientoParaProducto(group.Key),
                        VideosTutoriales = await GetVideosTutorialesParaProducto(group.Key),
                        DocumentosTecnicos = await GetDocumentosTecnicosParaProducto(group.Key),
                        LinksUtiles = await GetLinksUtilesParaProducto(group.Key)
                    };

                    result.Add(productDto);
                }

                return Ok(new { success = true, data = result.OrderByDescending(p => p.UltimaCompra) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener recursos de productos del cliente");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener recursos"
                });
            }
        }

        /// <summary>
        /// Obtiene recursos específicos para un producto comprado
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<ProductResourceDetailDto>> GetProductResources(int productId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                // Verificar que el cliente haya comprado este producto
                var hasPurchased = await _context.DetallesVenta
                    .AnyAsync(d => d.ProductoId == productId && d.Venta.UsuarioId == currentUserId);

                if (!hasPurchased)
                    return Forbid("No tienes acceso a los recursos de este producto");

                var producto = await _context.Productos.FindAsync(productId);
                if (producto == null)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                var resourceDetail = new ProductResourceDetailDto
                {
                    ProductoId = productId,
                    NombreProducto = producto.Nombre,
                    DescripcionProducto = producto.Descripcion,
                    ImagenPrincipal = producto.ImagenPrincipal,

                    // Información completa del producto
                    InformacionCompleta = new ProductFullInfoDto
                    {
                        DescripcionDetallada = producto.DescripcionDetallada,
                        Caracteristicas = DeserializeStringList(producto.Caracteristicas),
                        Beneficios = DeserializeStringList(producto.Beneficios),
                        ImagenesSecundarias = DeserializeStringList(producto.ImagenesSecundarias),
                        VideoDemo = producto.VideoDemo
                    },

                    // Recursos organizados por categoría - ACTUALIZADO PARA USAR DATOS REALES
                    Documentacion = new ProductDocumentationDto
                    {
                        ManualesUsuario = await GetManualesParaProducto(productId),
                        GuiasInstalacion = await GetGuiasInstalacionParaProducto(productId),
                        GuiasMantenimiento = await GetGuiasMantenimientoParaProducto(productId),
                        EspecificacionesTecnicas = await GetEspecificacionesTecnicasParaProducto(productId),
                        DiagramasEsquemas = await GetDiagramasParaProducto(productId)
                    },

                    MultimediaResources = new ProductMultimediaDto
                    {
                        VideosTutoriales = await GetVideosTutorialesParaProducto(productId),
                        VideosMantenimiento = await GetVideosMantenimientoParaProducto(productId),
                        AudioGuias = await GetAudioGuiasParaProducto(productId),
                        ImagenesReferencia = await GetImagenesReferenciaParaProducto(productId)
                    },

                    SoporteTecnico = new ProductSupportDto
                    {
                        ContactoSoporte = GetContactoSoporteParaProducto(productId),
                        FAQs = GetFAQsParaProducto(productId),
                        SolucionProblemas = GetSolucionProblemasParaProducto(productId),
                        LinksUtiles = await GetLinksUtilesParaProducto(productId)
                    }
                };

                return Ok(new { success = true, data = resourceDetail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener recursos del producto {ProductId}", productId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener recursos del producto"
                });
            }
        }

        /// <summary>
        /// Obtiene información de garantía para un producto comprado
        /// </summary>
        [HttpGet("warranty/{productId}")]
        public async Task<ActionResult<ProductWarrantyDto>> GetProductWarranty(int productId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                // Obtener información de compras del producto
                var compras = await _context.DetallesVenta
                    .Where(d => d.ProductoId == productId && d.Venta.UsuarioId == currentUserId)
                    .Include(d => d.Venta)
                    .Include(d => d.Producto)
                    .OrderByDescending(d => d.Venta.FechaVenta)
                    .ToListAsync();

                if (!compras.Any())
                    return Forbid("No tienes acceso a la información de garantía de este producto");

                var primeraCompra = compras.Last();
                var ultimaCompra = compras.First();
                var producto = primeraCompra.Producto;

                // Calcular información de garantía (ejemplo: 2 años desde la primera compra)
                var duracionGarantia = 24; // meses
                var fechaInicioGarantia = primeraCompra.Venta.FechaVenta;
                var fechaFinGarantia = fechaInicioGarantia.AddMonths(duracionGarantia);
                var diasRestantes = (fechaFinGarantia - DateTime.Now).Days;

                var warranty = new ProductWarrantyDto
                {
                    ProductoId = productId,
                    NombreProducto = producto.Nombre,
                    FechaCompra = primeraCompra.Venta.FechaVenta,
                    NumeroVenta = primeraCompra.Venta.NumeroVenta,
                    DuracionGarantiaMeses = duracionGarantia,
                    FechaInicioGarantia = fechaInicioGarantia,
                    FechaFinGarantia = fechaFinGarantia,
                    DiasRestantes = Math.Max(0, diasRestantes),
                    GarantiaVigente = diasRestantes > 0,
                    TerminosGarantia = GetTerminosGarantiaParaProducto(productId),
                    CoberturasIncluidas = GetCoberturasGarantiaParaProducto(productId),
                    ExclusionesGarantia = GetExclusionesGarantiaParaProducto(productId),
                    ProcedimientoReclamacion = GetProcedimientoReclamacionParaProducto(productId)
                };

                return Ok(new { success = true, data = warranty });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de garantía del producto {ProductId}", productId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener información de garantía"
                });
            }
        }

        #region Métodos auxiliares actualizados para usar ProductoDocumentos de la base de datos

        private async Task<List<ResourceItemDto>> GetManualesParaProducto(int productId)
        {
            return await _context.ProductoDocumentos
                .Where(d => d.ProductoId == productId &&
                           d.EsVisibleParaCliente &&
                           (d.Tipo == "Manual" || d.Tipo == "Guia"))
                .Select(d => new ResourceItemDto
                {
                    Titulo = d.Titulo,
                    Descripcion = d.Titulo,
                    TipoRecurso = d.Tipo,
                    Url = d.Url.StartsWith("http") ? d.Url : $"/api/product-documents/download/{d.Id}",
                    Tamano = "",
                    FechaActualizacion = d.FechaCreacion
                })
                .ToListAsync();
        }

        private async Task<List<ResourceItemDto>> GetGuiasMantenimientoParaProducto(int productId)
        {
            return await _context.ProductoDocumentos
                .Where(d => d.ProductoId == productId &&
                           d.EsVisibleParaCliente &&
                           d.Tipo == "Mantenimiento")
                .Select(d => new ResourceItemDto
                {
                    Titulo = d.Titulo,
                    Descripcion = d.Titulo,
                    TipoRecurso = d.Tipo,
                    Url = d.Url.StartsWith("http") ? d.Url : $"/api/product-documents/download/{d.Id}",
                    Tamano = "",
                    FechaActualizacion = d.FechaCreacion
                })
                .ToListAsync();
        }

        private async Task<List<ResourceItemDto>> GetVideosTutorialesParaProducto(int productId)
        {
            return await _context.ProductoDocumentos
                .Where(d => d.ProductoId == productId &&
                           d.EsVisibleParaCliente &&
                           d.Tipo == "Video")
                .Select(d => new ResourceItemDto
                {
                    Titulo = d.Titulo,
                    Descripcion = d.Titulo,
                    TipoRecurso = "Video",
                    Url = d.Url,
                    Tamano = "",
                    FechaActualizacion = d.FechaCreacion
                })
                .ToListAsync();
        }

        private async Task<List<ResourceItemDto>> GetDocumentosTecnicosParaProducto(int productId)
        {
            return await _context.ProductoDocumentos
                .Where(d => d.ProductoId == productId &&
                           d.EsVisibleParaCliente &&
                           d.Tipo == "Especificaciones")
                .Select(d => new ResourceItemDto
                {
                    Titulo = d.Titulo,
                    Descripcion = d.Titulo,
                    TipoRecurso = d.Tipo,
                    Url = d.Url.StartsWith("http") ? d.Url : $"/api/product-documents/download/{d.Id}",
                    Tamano = "",
                    FechaActualizacion = d.FechaCreacion
                })
                .ToListAsync();
        }

        private async Task<List<ResourceItemDto>> GetLinksUtilesParaProducto(int productId)
        {
            return await _context.ProductoDocumentos
                .Where(d => d.ProductoId == productId &&
                           d.EsVisibleParaCliente &&
                           (d.Tipo == "Tutorial" || d.Tipo == "Soporte" || d.Tipo == "Comunidad"))
                .Select(d => new ResourceItemDto
                {
                    Titulo = d.Titulo,
                    Descripcion = d.Titulo,
                    TipoRecurso = "Link",
                    Url = d.Url,
                    Tamano = "",
                    FechaActualizacion = d.FechaCreacion
                })
                .ToListAsync();
        }

        private async Task<List<ResourceItemDto>> GetGuiasInstalacionParaProducto(int productId)
        {
            return await _context.ProductoDocumentos
                .Where(d => d.ProductoId == productId &&
                           d.EsVisibleParaCliente &&
                           d.Tipo == "Guia")
                .Select(d => new ResourceItemDto
                {
                    Titulo = d.Titulo,
                    Descripcion = d.Titulo,
                    TipoRecurso = d.Tipo,
                    Url = d.Url.StartsWith("http") ? d.Url : $"/api/product-documents/download/{d.Id}",
                    Tamano = "",
                    FechaActualizacion = d.FechaCreacion
                })
                .ToListAsync();
        }

        private async Task<List<ResourceItemDto>> GetEspecificacionesTecnicasParaProducto(int productId)
        {
            return await _context.ProductoDocumentos
                .Where(d => d.ProductoId == productId &&
                           d.EsVisibleParaCliente &&
                           d.Tipo == "Especificaciones")
                .Select(d => new ResourceItemDto
                {
                    Titulo = d.Titulo,
                    Descripcion = d.Titulo,
                    TipoRecurso = d.Tipo,
                    Url = d.Url.StartsWith("http") ? d.Url : $"/api/product-documents/download/{d.Id}",
                    Tamano = "",
                    FechaActualizacion = d.FechaCreacion
                })
                .ToListAsync();
        }

        private async Task<List<ResourceItemDto>> GetDiagramasParaProducto(int productId)
        {
            return await _context.ProductoDocumentos
                .Where(d => d.ProductoId == productId &&
                           d.EsVisibleParaCliente &&
                           d.Tipo == "Diagrama")
                .Select(d => new ResourceItemDto
                {
                    Titulo = d.Titulo,
                    Descripcion = d.Titulo,
                    TipoRecurso = d.Tipo,
                    Url = d.Url.StartsWith("http") ? d.Url : $"/api/product-documents/download/{d.Id}",
                    Tamano = "",
                    FechaActualizacion = d.FechaCreacion
                })
                .ToListAsync();
        }

        private async Task<List<ResourceItemDto>> GetVideosMantenimientoParaProducto(int productId)
        {
            return await _context.ProductoDocumentos
                .Where(d => d.ProductoId == productId &&
                           d.EsVisibleParaCliente &&
                           d.Tipo == "Video" &&
                           d.Titulo.ToLower().Contains("mantenimiento"))
                .Select(d => new ResourceItemDto
                {
                    Titulo = d.Titulo,
                    Descripcion = d.Titulo,
                    TipoRecurso = "Video",
                    Url = d.Url,
                    Tamano = "",
                    FechaActualizacion = d.FechaCreacion
                })
                .ToListAsync();
        }

        private async Task<List<ResourceItemDto>> GetAudioGuiasParaProducto(int productId)
        {
            return await _context.ProductoDocumentos
                .Where(d => d.ProductoId == productId &&
                           d.EsVisibleParaCliente &&
                           d.Tipo == "Audio")
                .Select(d => new ResourceItemDto
                {
                    Titulo = d.Titulo,
                    Descripcion = d.Titulo,
                    TipoRecurso = "Audio",
                    Url = d.Url,
                    Tamano = "",
                    FechaActualizacion = d.FechaCreacion
                })
                .ToListAsync();
        }

        private async Task<List<ResourceItemDto>> GetImagenesReferenciaParaProducto(int productId)
        {
            return await _context.ProductoDocumentos
                .Where(d => d.ProductoId == productId &&
                           d.EsVisibleParaCliente &&
                           d.Tipo == "Galería")
                .Select(d => new ResourceItemDto
                {
                    Titulo = d.Titulo,
                    Descripcion = d.Titulo,
                    TipoRecurso = "Galería",
                    Url = d.Url,
                    Tamano = "",
                    FechaActualizacion = d.FechaCreacion
                })
                .ToListAsync();
        }

        // Métodos auxiliares que permanecen igual (datos estáticos para soporte, etc.)
        private ProductSupportContactDto GetContactoSoporteParaProducto(int productId)
        {
            return new ProductSupportContactDto
            {
                TelefonoSoporte = "+52 (477) 123-4567",
                EmailSoporte = "soporte@smartagro.com",
                HorarioAtencion = "Lunes a Viernes 8:00 AM - 6:00 PM",
                ChatOnline = "https://chat.smartagro.com",
                WhatsApp = "+52 477 123 4567"
            };
        }

        private List<FAQItemDto> GetFAQsParaProducto(int productId)
        {
            return new List<FAQItemDto>
            {
                new FAQItemDto
                {
                    Pregunta = "¿Cómo resetear el sistema?",
                    Respuesta = "Para resetear el sistema, mantenga presionado el botón de reset por 10 segundos hasta que parpadee la luz azul."
                },
                new FAQItemDto
                {
                    Pregunta = "¿Qué hacer si los sensores no responden?",
                    Respuesta = "Verifique las conexiones y la alimentación eléctrica. Si el problema persiste, contacte soporte técnico."
                }
            };
        }

        private List<TroubleshootingItemDto> GetSolucionProblemasParaProducto(int productId)
        {
            return new List<TroubleshootingItemDto>
            {
                new TroubleshootingItemDto
                {
                    Problema = "El sistema no se conecta a WiFi",
                    Solucion = "1. Verifique que el WiFi esté funcionando\n2. Asegúrese de que la contraseña sea correcta\n3. Reinicie el router\n4. Contacte soporte si persiste"
                },
                new TroubleshootingItemDto
                {
                    Problema = "Los sensores muestran valores incorrectos",
                    Solucion = "1. Limpie los sensores con un paño suave\n2. Calibre los sensores siguiendo el manual\n3. Verifique que no haya obstrucciones"
                }
            };
        }

        private List<string> GetTerminosGarantiaParaProducto(int productId)
        {
            return new List<string>
            {
                "Garantía válida por 24 meses desde la fecha de compra",
                "Cubre defectos de fabricación y materiales",
                "Incluye mano de obra especializada",
                "Reemplazo gratuito de componentes defectuosos"
            };
        }

        private List<string> GetCoberturasGarantiaParaProducto(int productId)
        {
            return new List<string>
            {
                "Sistema de control principal",
                "Sensores IoT (humedad, temperatura, pH)",
                "Válvulas de riego automáticas",
                "Módulo de comunicación WiFi",
                "Software y aplicación móvil"
            };
        }

        private List<string> GetExclusionesGarantiaParaProducto(int productId)
        {
            return new List<string>
            {
                "Daños por mal uso o negligencia",
                "Exposición a condiciones climáticas extremas",
                "Modificaciones no autorizadas",
                "Daños por sobre voltaje o problemas eléctricos",
                "Desgaste normal por uso"
            };
        }

        private string GetProcedimientoReclamacionParaProducto(int productId)
        {
            return @"Para realizar una reclamación de garantía:
1. Contacte nuestro servicio técnico al +52 (477) 123-4567
2. Proporcione el número de venta y descripción del problema
3. Envíe fotos o videos del problema si es posible
4. Un técnico evaluará el caso en 24-48 horas
5. Se programará visita técnica o reemplazo según corresponda";
        }

        private List<string> DeserializeStringList(string? jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonString) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        #endregion
    }
}

// DTOs para recursos del cliente (estos permanecen igual)
namespace SmartAgro.Models.DTOs
{
    public class ClientProductResourceDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? DescripcionProducto { get; set; }
        public string? DescripcionDetallada { get; set; }
        public string? ImagenPrincipal { get; set; }
        public List<string>? ImagenesSecundarias { get; set; }
        public string? VideoDemo { get; set; }
        public List<string>? Caracteristicas { get; set; }
        public List<string>? Beneficios { get; set; }

        // Información de compra
        public DateTime PrimeraCompra { get; set; }
        public DateTime UltimaCompra { get; set; }
        public int TotalComprado { get; set; }

        // Recursos disponibles
        public List<ResourceItemDto> ManualesDisponibles { get; set; } = new List<ResourceItemDto>();
        public List<ResourceItemDto> GuiasMantenimiento { get; set; } = new List<ResourceItemDto>();
        public List<ResourceItemDto> VideosTutoriales { get; set; } = new List<ResourceItemDto>();
        public List<ResourceItemDto> DocumentosTecnicos { get; set; } = new List<ResourceItemDto>();
        public List<ResourceItemDto> LinksUtiles { get; set; } = new List<ResourceItemDto>();
    }

    public class ProductResourceDetailDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? DescripcionProducto { get; set; }
        public string? ImagenPrincipal { get; set; }

        public ProductFullInfoDto InformacionCompleta { get; set; } = new ProductFullInfoDto();
        public ProductDocumentationDto Documentacion { get; set; } = new ProductDocumentationDto();
        public ProductMultimediaDto MultimediaResources { get; set; } = new ProductMultimediaDto();
        public ProductSupportDto SoporteTecnico { get; set; } = new ProductSupportDto();
    }

    public class ProductFullInfoDto
    {
        public string? DescripcionDetallada { get; set; }
        public List<string>? Caracteristicas { get; set; }
        public List<string>? Beneficios { get; set; }
        public List<string>? ImagenesSecundarias { get; set; }
        public string? VideoDemo { get; set; }
    }

    public class ProductDocumentationDto
    {
        public List<ResourceItemDto> ManualesUsuario { get; set; } = new List<ResourceItemDto>();
        public List<ResourceItemDto> GuiasInstalacion { get; set; } = new List<ResourceItemDto>();
        public List<ResourceItemDto> GuiasMantenimiento { get; set; } = new List<ResourceItemDto>();
        public List<ResourceItemDto> EspecificacionesTecnicas { get; set; } = new List<ResourceItemDto>();
        public List<ResourceItemDto> DiagramasEsquemas { get; set; } = new List<ResourceItemDto>();
    }

    public class ProductMultimediaDto
    {
        public List<ResourceItemDto> VideosTutoriales { get; set; } = new List<ResourceItemDto>();
        public List<ResourceItemDto> VideosMantenimiento { get; set; } = new List<ResourceItemDto>();
        public List<ResourceItemDto> AudioGuias { get; set; } = new List<ResourceItemDto>();
        public List<ResourceItemDto> ImagenesReferencia { get; set; } = new List<ResourceItemDto>();
    }

    public class ProductSupportDto
    {
        public ProductSupportContactDto ContactoSoporte { get; set; } = new ProductSupportContactDto();
        public List<FAQItemDto> FAQs { get; set; } = new List<FAQItemDto>();
        public List<TroubleshootingItemDto> SolucionProblemas { get; set; } = new List<TroubleshootingItemDto>();
        public List<ResourceItemDto> LinksUtiles { get; set; } = new List<ResourceItemDto>();
    }

    public class ResourceItemDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string TipoRecurso { get; set; } = string.Empty; // PDF, Video, Audio, Link, etc.
        public string Url { get; set; } = string.Empty;
        public string? Tamano { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }

    public class ProductSupportContactDto
    {
        public string TelefonoSoporte { get; set; } = string.Empty;
        public string EmailSoporte { get; set; } = string.Empty;
        public string HorarioAtencion { get; set; } = string.Empty;
        public string? ChatOnline { get; set; }
        public string? WhatsApp { get; set; }
    }

    public class FAQItemDto
    {
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;
    }

    public class TroubleshootingItemDto
    {
        public string Problema { get; set; } = string.Empty;
        public string Solucion { get; set; } = string.Empty;
    }

    public class ProductWarrantyDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public DateTime FechaCompra { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public int DuracionGarantiaMeses { get; set; }
        public DateTime FechaInicioGarantia { get; set; }
        public DateTime FechaFinGarantia { get; set; }
        public int DiasRestantes { get; set; }
        public bool GarantiaVigente { get; set; }

        public List<string> TerminosGarantia { get; set; } = new List<string>();
        public List<string> CoberturasIncluidas { get; set; } = new List<string>();
        public List<string> ExclusionesGarantia { get; set; } = new List<string>();
        public string ProcedimientoReclamacion { get; set; } = string.Empty;
    }
}