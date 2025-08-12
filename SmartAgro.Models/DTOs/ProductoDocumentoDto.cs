// SmartAgro.Models/DTOs/DocumentDTOs.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartAgro.Models.DTOs
{
    public class AgregarDocumentoDto
    {
        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Tipo { get; set; } = string.Empty; // PDF, Video, Link

        [Required]
        public string Url { get; set; } = string.Empty;
    }

    public class UploadDocumentDto
    {
        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Tipo { get; set; } = string.Empty; // PDF, Manual, Guia, Especificaciones

        public string? Descripcion { get; set; }

        [Required]
        public IFormFile Archivo { get; set; } = null!;

        public bool EsVisibleParaCliente { get; set; } = true;
    }

    public class AddLinkDocumentDto
    {
        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Tipo { get; set; } = string.Empty; // Video, Link, Tutorial

        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool EsVisibleParaCliente { get; set; } = true;
    }

    public class UpdateDocumentDto
    {
        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Tipo { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool EsVisibleParaCliente { get; set; } = true;
    }

    public class ProductDocumentDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool EsVisibleParaCliente { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? NombreArchivo { get; set; }
        public long? TamanoArchivo { get; set; }
    }
}