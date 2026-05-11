using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Database.Models;

public class Talla
{
    [Key]
    public int id_talla { get; set; }
    [Required]
    [MaxLength(50)]
    public string nombre_talla { get; set; } // e.g. "38", "39", "S", "M"
}
