using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Database.Models;

public class Color
{
    [Key]
    public int id_color { get; set; }
    [Required]
    [MaxLength(50)]
    public string nombre_color { get; set; } // e.g. "Negro", "Blanco", "Rojo"
    [MaxLength(7)]
    public string? codigo_hex { get; set; } // e.g. "#000000"
}
