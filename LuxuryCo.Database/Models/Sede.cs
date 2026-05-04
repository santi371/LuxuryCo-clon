using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class Sede
{
    [Key]
    public int id_sede { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string nombre { get; set; }

    [Required]
    [MaxLength(100)]
    public string ciudad { get; set; }

    [MaxLength(255)]
    public string? direccion { get; set; }

    [MaxLength(50)]
    public string? telefono { get; set; }

    public bool activa { get; set; } = true;
    
    public DateTime fecha_creacion { get; set; } = DateTime.UtcNow;
}
