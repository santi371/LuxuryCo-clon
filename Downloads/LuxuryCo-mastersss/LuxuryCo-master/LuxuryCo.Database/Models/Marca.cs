using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class Marca
{
    [Key]
    public int id_marca { get; set; }
    [Required]
    [MaxLength(100)]
    public string nombre { get; set; }
    [MaxLength(200)]
    public string? descripcion { get; set; }
    
    [MaxLength(500)]
    public string? logo_url { get; set; }

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
