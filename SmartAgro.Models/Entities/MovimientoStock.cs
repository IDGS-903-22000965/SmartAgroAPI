// SmartAgro.Models.Entities
using SmartAgro.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

public class MovimientoStock
{
    public int Id { get; set; }

    public int MateriaPrimaId { get; set; }
    public virtual MateriaPrima MateriaPrima { get; set; } = null!;

    public string Tipo { get; set; } = string.Empty; // Entrada, Salida, Ajuste

    public decimal Cantidad { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CostoUnitario { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public string? Referencia { get; set; } // Ej: CP-202501-001
    public string? Observaciones { get; set; }
}