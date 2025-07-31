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
                    .Select(g => new ClientProductResourceDto
                    {
                        ProductoId = g.Key,
                        NombreProducto = g.First().Producto.Nombre,
                        DescripcionProducto = g.First().Producto.Descripcion,
                        DescripcionDetallada = g.First().Producto.DescripcionDetallada,
                        ImagenPrincipal = g.First().Producto.ImagenPrincipal,
                        ImagenesSecundarias = DeserializeStringList(g.First().Producto.ImagenesSecundarias),
                        VideoDemo = g.First().Producto.VideoDemo,
                        Caracteristicas = DeserializeStringList(g.First().Producto.Caracteristicas),
                        Beneficios = DeserializeStringList(g.First().Producto.Beneficios),

                        // Información de compra
                        PrimeraCompra = g.Min(d => d.Venta.FechaVenta),
                        UltimaCompra = g.Max(d => d.Venta.FechaVenta),
                        TotalComprado = g.Sum(d => d.Cantidad),

                        // Recursos y documentación
                        ManualesDisponibles = GetManualesParaProducto(g.Key),
                        GuiasMantenimiento = GetGuiasMantenimientoParaProducto(g.Key),
                        VideosTutoriales = GetVideosTutorialesParaProducto(g.Key),
                        DocumentosTecnicos = GetDocumentosTecnicosParaProducto(g.Key),
                        LinksUtiles = GetLinksUtilesParaProducto(g.Key)
                    })
                    .OrderByDescending(p => p.UltimaCompra)
                    .ToListAsync();

                return Ok(new { success = true, data = ownedProducts });
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

                    // Recursos organizados por categoría
                    Documentacion = new ProductDocumentationDto
                    {
                        ManualesUsuario = GetManualesParaProducto(productId),
                        GuiasInstalacion = GetGuiasInstalacionParaProducto(productId),
                        GuiasMantenimiento = GetGuiasMantenimientoParaProducto(productId),
                        EspecificacionesTecnicas = GetEspecificacionesTecnicasParaProducto(productId),
                        DiagramasEsquemas = GetDiagramasParaProducto(productId)
                    },

                    MultimediaResources = new ProductMultimediaDto
                    {
                        VideosTutoriales = GetVideosTutorialesParaProducto(productId),
                        VideosMantenimiento = GetVideosMantenimientoParaProducto(productId),
                        AudioGuias = GetAudioGuiasParaProducto(productId),
                        ImagenesReferencia = GetImagenesReferenciaParaProducto(productId)
                    },

                    SoporteTecnico = new ProductSupportDto
                    {
                        ContactoSoporte = GetContactoSoporteParaProducto(productId),
                        FAQs = GetFAQsParaProducto(productId),
                        SolucionProblemas = GetSolucionProblemasParaProducto(productId),
                        LinksUtiles = GetLinksUtilesParaProducto(productId)
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

        #region Métodos auxiliares para obtener recursos (estos pueden ser configurados por los administradores)

        private List<ResourceItemDto> GetManualesParaProducto(int productId)
        {
            // En una implementación real, estos datos vendrían de la base de datos
            return new List<ResourceItemDto>
            {
                new ResourceItemDto
                {
                    Titulo = "Manual de Usuario SmartAgro IoT",
                    Descripcion = "Guía completa para el uso del sistema de riego inteligente",
                    TipoRecurso = "PDF",
                    Url = "/resources/manuals/smartagro-manual-usuario.pdf",
                    Tamano  = "2.5 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-30)
                },
                new ResourceItemDto
                {
                    Titulo = "Guía de Inicio Rápido",
                    Descripcion = "Pasos básicos para configurar tu sistema",
                    TipoRecurso = "PDF",
                    Url = "/resources/manuals/guia-inicio-rapido.pdf",
                    Tamano = "1.2 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-15)
                }
            };
        }

        private List<ResourceItemDto> GetGuiasMantenimientoParaProducto(int productId)
        {
            return new List<ResourceItemDto>
            {
                new ResourceItemDto
                {
                    Titulo = "Mantenimiento Preventivo Mensual",
                    Descripcion = "Tareas de mantenimiento a realizar cada mes",
                    TipoRecurso = "PDF",
                    Url = "/resources/maintenance/mantenimiento-mensual.pdf",
                    Tamano = "800 KB",
                    FechaActualizacion = DateTime.Now.AddDays(-20)
                },
                new ResourceItemDto
                {
                    Titulo = "Limpieza de Sensores",
                    Descripcion = "Procedimiento para limpieza de sensores IoT",
                    TipoRecurso = "Video",
                    Url = "/resources/videos/limpieza-sensores.mp4",
                    Tamano = "15 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-10)
                }
            };
        }

        private List<ResourceItemDto> GetVideosTutorialesParaProducto(int productId)
        {
            return new List<ResourceItemDto>
            {
                new ResourceItemDto
                {
                    Titulo = "Configuración Inicial del Sistema",
                    Descripcion = "Video tutorial paso a paso para configurar tu sistema SmartAgro",
                    TipoRecurso = "Video",
                    Url = "/resources/videos/configuracion-inicial.mp4",
                    Tamano = "25 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-5)
                },
                new ResourceItemDto
                {
                    Titulo = "Uso de la Aplicación Móvil",
                    Descripcion = "Cómo utilizar la app móvil para controlar tu sistema",
                    TipoRecurso = "Video",
                    Url = "/resources/videos/app-movil-tutorial.mp4",
                    Tamano = "18 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-3)
                }
            };
        }

        private List<ResourceItemDto> GetDocumentosTecnicosParaProducto(int productId)
        {
            return new List<ResourceItemDto>
            {
                new ResourceItemDto
                {
                    Titulo = "Especificaciones Técnicas Completas",
                    Descripcion = "Detalle completo de especificaciones técnicas del sistema",
                    TipoRecurso = "PDF",
                    Url = "/resources/technical/especificaciones-tecnicas.pdf",
                    Tamano = "3.2 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-45)
                },
                new ResourceItemDto
                {
                    Titulo = "Diagramas de Conexión",
                    Descripcion = "Esquemas eléctricos y de conexión del sistema",
                    TipoRecurso = "PDF",
                    Url = "/resources/technical/diagramas-conexion.pdf",
                    Tamano = "1.8 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-30)
                }
            };
        }

        private List<ResourceItemDto> GetLinksUtilesParaProducto(int productId)
        {
            return new List<ResourceItemDto>
            {
                new ResourceItemDto
                {
                    Titulo = "Portal de Soporte Online",
                    Descripcion = "Acceso al portal de soporte técnico 24/7",
                    TipoRecurso = "Link",
                    Url = "https://soporte.smartagro.com",
                    Tamano = "",
                    FechaActualizacion = DateTime.Now.AddDays(-1)
                },
                new ResourceItemDto
                {
                    Titulo = "Comunidad de Usuarios",
                    Descripcion = "Foro de usuarios para compartir experiencias",
                    TipoRecurso = "Link",
                    Url = "https://comunidad.smartagro.com",
                    Tamano = "",
                    FechaActualizacion = DateTime.Now.AddDays(-2)
                }
            };
        }

        private List<ResourceItemDto> GetGuiasInstalacionParaProducto(int productId)
        {
            return new List<ResourceItemDto>
            {
                new ResourceItemDto
                {
                    Titulo = "Guía de Instalación Paso a Paso",
                    Descripcion = "Instrucciones detalladas para la instalación del sistema",
                    TipoRecurso = "PDF",
                    Url = "/resources/installation/guia-instalacion.pdf",
                    Tamano = "4.1 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-60)
                }
            };
        }

        private List<ResourceItemDto> GetEspecificacionesTecnicasParaProducto(int productId)
        {
            return new List<ResourceItemDto>
            {
                new ResourceItemDto
                {
                    Titulo = "Ficha Técnica del Producto",
                    Descripcion = "Especificaciones técnicas resumidas",
                    TipoRecurso = "PDF",
                    Url = "/resources/specs/ficha-tecnica.pdf",
                    Tamano = "1.5 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-30)
                }
            };
        }

        private List<ResourceItemDto> GetDiagramasParaProducto(int productId)
        {
            return new List<ResourceItemDto>
            {
                new ResourceItemDto
                {
                    Titulo = "Diagrama del Sistema Completo",
                    Descripcion = "Esquema general del sistema de riego",
                    TipoRecurso = "PDF",
                    Url = "/resources/diagrams/sistema-completo.pdf",
                    Tamano = "2.3 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-45)
                }
            };
        }

        private List<ResourceItemDto> GetVideosMantenimientoParaProducto(int productId)
        {
            return new List<ResourceItemDto>
            {
                new ResourceItemDto
                {
                    Titulo = "Mantenimiento Trimestral",
                    Descripcion = "Video guía para mantenimiento cada 3 meses",
                    TipoRecurso = "Video",
                    Url = "/resources/videos/mantenimiento-trimestral.mp4",
                    Tamano = "22 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-20)
                }
            };
        }

        private List<ResourceItemDto> GetAudioGuiasParaProducto(int productId)
        {
            return new List<ResourceItemDto>
            {
                new ResourceItemDto
                {
                    Titulo = "Guía de Audio para Configuración",
                    Descripcion = "Instrucciones de voz para configuración básica",
                    TipoRecurso = "Audio",
                    Url = "/resources/audio/configuracion-basica.mp3",
                    Tamano = "8 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-25)
                }
            };
        }

        private List<ResourceItemDto> GetImagenesReferenciaParaProducto(int productId)
        {
            return new List<ResourceItemDto>
            {
                new ResourceItemDto
                {
                    Titulo = "Galería de Instalaciones",
                    Descripcion = "Imágenes de referencia de instalaciones exitosas",
                    TipoRecurso = "Galería",
                    Url = "/resources/gallery/instalaciones",
                    Tamano = "5.2 MB",
                    FechaActualizacion = DateTime.Now.AddDays(-15)
                }
            };
        }

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

// DTOs para recursos del cliente
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