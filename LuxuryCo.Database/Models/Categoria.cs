using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Database.Models;

public class Categoria
{
    [Key]
    public int id_categoria { get; set; }
    [Required]
    [MaxLength(100)]
    public string nombre { get; set; }
    [MaxLength(200)]
    public string? descripcion { get; set; }

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
