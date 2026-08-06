using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class ImagenProducto
{
    [Key]
    public int id_imagen { get; set; }
    public int? id_producto { get; set; }
    public string? url_imagen { get; set; }
    public bool principal { get; set; } = false;
    public int orden { get; set; } = 0;

    [ForeignKey("id_producto")]
    public Producto Producto { get; set; }
}
