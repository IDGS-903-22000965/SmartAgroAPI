// SmartAgro.API/Controllers/ProductDocumentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.DTOs;
using SmartAgro.Models.Entities;
using System.Security.Claims;

namespace SmartAgro.API.Controllers
{
    [ApiController]
    [Route("api/product-documents")]
    [Authorize(Roles = "Admin")]
    public class ProductDocumentsController : ControllerBase
    {
        private readonly SmartAgroDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductDocumentsController> _logger;

        public ProductDocumentsController(
            SmartAgroDbContext context,
            IWebHostEnvironment environment,
            ILogger<ProductDocumentsController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los documentos de un producto
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<List<ProductDocumentDto>>> GetProductDocuments(int productId)
        {
            try
            {
                var documents = await _context.ProductoDocumentos
                    .Where(d => d.ProductoId == productId)
                    .OrderBy(d => d.Tipo)
                    .ThenBy(d => d.Titulo)
                    .Select(d => new ProductDocumentDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        Titulo = d.Titulo,
                        Tipo = d.Tipo,
                        Url = d.Url,
                        EsVisibleParaCliente = d.EsVisibleParaCliente,
                        FechaCreacion = d.FechaCreacion,
                        NombreArchivo = d.Url.Contains("/uploads/") ? Path.GetFileName(d.Url) : null
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = documents });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documentos del producto {ProductId}", productId);
                return StatusCode(500, new { success = false, message = "Error al obtener documentos" });
            }
        }

        /// <summary>
        /// Sube un archivo (PDF, DOC, etc.) para un producto
        /// </summary>
        [HttpPost("upload/{productId}")]
        public async Task<ActionResult> UploadDocument(int productId, [FromForm] UploadDocumentDto uploadDto)
        {
            try
            {
                // Verificar que el producto existe
                var producto = await _context.Productos.FindAsync(productId);
                if (producto == null)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                // Validar archivo
                if (uploadDto.Archivo == null || uploadDto.Archivo.Length == 0)
                    return BadRequest(new { success = false, message = "No se proporcionó archivo" });

                // Validar tipo de archivo
                var allowedTypes = new[] { ".pdf", ".doc", ".docx", ".txt", ".rtf" };
                var fileExtension = Path.GetExtension(uploadDto.Archivo.FileName).ToLowerInvariant();

                if (!allowedTypes.Contains(fileExtension))
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tipo de archivo no permitido. Solo se permiten: PDF, DOC, DOCX, TXT, RTF"
                    });

                // Validar tamaño (máximo 10MB)
                if (uploadDto.Archivo.Length > 10 * 1024 * 1024)
                    return BadRequest(new { success = false, message = "El archivo no puede ser mayor a 10MB" });

                // Crear directorio si no existe
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "product-documents");
                Directory.CreateDirectory(uploadsPath);

                // Generar nombre único para el archivo
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var fileName = $"{productId}_{timestamp}_{Guid.NewGuid().ToString()[..8]}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Guardar archivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadDto.Archivo.CopyToAsync(stream);
                }

                // Crear URL relativa para acceder al archivo
                var fileUrl = $"/uploads/product-documents/{fileName}";

                // Crear registro en base de datos
                var documento = new ProductoDocumento
                {
                    ProductoId = productId,
                    Titulo = uploadDto.Titulo,
                    Tipo = uploadDto.Tipo,
                    Url = fileUrl,
                    EsVisibleParaCliente = uploadDto.EsVisibleParaCliente,
                    FechaCreacion = DateTime.Now
                };

                _context.ProductoDocumentos.Add(documento);
                await _context.SaveChangesAsync();

                var documentDto = new ProductDocumentDto
                {
                    Id = documento.Id,
                    ProductoId = documento.ProductoId,
                    Titulo = documento.Titulo,
                    Tipo = documento.Tipo,
                    Url = documento.Url,
                    EsVisibleParaCliente = documento.EsVisibleParaCliente,
                    FechaCreacion = documento.FechaCreacion,
                    NombreArchivo = fileName,
                    TamanoArchivo = uploadDto.Archivo.Length
                };

                return Ok(new
                {
                    success = true,
                    message = "Documento subido exitosamente",
                    data = documentDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir documento para producto {ProductId}", productId);
                return StatusCode(500, new { success = false, message = "Error al subir documento" });
            }
        }

        /// <summary>
        /// Agrega un enlace (video de YouTube, link externo, etc.) para un producto
        /// </summary>
        [HttpPost("add-link/{productId}")]
        public async Task<ActionResult> AddLinkDocument(int productId, AddLinkDocumentDto linkDto)
        {
            try
            {
                // Verificar que el producto existe
                var producto = await _context.Productos.FindAsync(productId);
                if (producto == null)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                // Crear registro en base de datos
                var documento = new ProductoDocumento
                {
                    ProductoId = productId,
                    Titulo = linkDto.Titulo,
                    Tipo = linkDto.Tipo,
                    Url = linkDto.Url,
                    EsVisibleParaCliente = linkDto.EsVisibleParaCliente,
                    FechaCreacion = DateTime.Now
                };

                _context.ProductoDocumentos.Add(documento);
                await _context.SaveChangesAsync();

                var documentDto = new ProductDocumentDto
                {
                    Id = documento.Id,
                    ProductoId = documento.ProductoId,
                    Titulo = documento.Titulo,
                    Tipo = documento.Tipo,
                    Url = documento.Url,
                    EsVisibleParaCliente = documento.EsVisibleParaCliente,
                    FechaCreacion = documento.FechaCreacion
                };

                return Ok(new
                {
                    success = true,
                    message = "Enlace agregado exitosamente",
                    data = documentDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar enlace para producto {ProductId}", productId);
                return StatusCode(500, new { success = false, message = "Error al agregar enlace" });
            }
        }

        /// <summary>
        /// Actualiza un documento
        /// </summary>
        [HttpPut("{documentId}")]
        public async Task<ActionResult> UpdateDocument(int documentId, UpdateDocumentDto updateDto)
        {
            try
            {
                var documento = await _context.ProductoDocumentos.FindAsync(documentId);
                if (documento == null)
                    return NotFound(new { success = false, message = "Documento no encontrado" });

                documento.Titulo = updateDto.Titulo;
                documento.Tipo = updateDto.Tipo;
                documento.EsVisibleParaCliente = updateDto.EsVisibleParaCliente;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Documento actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar documento {DocumentId}", documentId);
                return StatusCode(500, new { success = false, message = "Error al actualizar documento" });
            }
        }

        /// <summary>
        /// Elimina un documento
        /// </summary>
        [HttpDelete("{documentId}")]
        public async Task<ActionResult> DeleteDocument(int documentId)
        {
            try
            {
                var documento = await _context.ProductoDocumentos.FindAsync(documentId);
                if (documento == null)
                    return NotFound(new { success = false, message = "Documento no encontrado" });

                // Si es un archivo local, eliminarlo del sistema de archivos
                if (documento.Url.StartsWith("/uploads/"))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, documento.Url.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.ProductoDocumentos.Remove(documento);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Documento eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar documento {DocumentId}", documentId);
                return StatusCode(500, new { success = false, message = "Error al eliminar documento" });
            }
        }

        /// <summary>
        /// Descarga un archivo - verificar que el cliente haya comprado el producto
        /// </summary>
        [HttpGet("download/{documentId}")]
        [AllowAnonymous]
        public async Task<ActionResult> DownloadDocument(int documentId)
        {
            try
            {
                var documento = await _context.ProductoDocumentos.FindAsync(documentId);
                if (documento == null)
                    return NotFound(new { success = false, message = "Documento no encontrado" });

                // Si el usuario está autenticado y es cliente, verificar que haya comprado el producto
                if (User.Identity?.IsAuthenticated == true && User.IsInRole("Cliente"))
                {
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(currentUserId))
                    {
                        var hasPurchased = await _context.DetallesVenta
                            .AnyAsync(d => d.ProductoId == documento.ProductoId &&
                                         d.Venta.UsuarioId == currentUserId);

                        if (!hasPurchased && documento.EsVisibleParaCliente)
                            return Forbid("No tienes acceso a este documento");
                    }
                }
                // Si no está autenticado o no es cliente, solo permitir acceso a documentos públicos
                else if (!documento.EsVisibleParaCliente)
                {
                    return Forbid("Documento no disponible públicamente");
                }

                // Si es un enlace externo, redirigir
                if (!documento.Url.StartsWith("/uploads/"))
                    return Redirect(documento.Url);

                // Si es un archivo local, descargarlo
                var filePath = Path.Combine(_environment.WebRootPath, documento.Url.TrimStart('/'));
                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { success = false, message = "Archivo no encontrado" });

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = Path.GetFileName(filePath);
                var contentType = GetContentType(fileName);

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar documento {DocumentId}", documentId);
                return StatusCode(500, new { success = false, message = "Error al descargar documento" });
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".rtf" => "application/rtf",
                _ => "application/octet-stream"
            };
        }
    }
}