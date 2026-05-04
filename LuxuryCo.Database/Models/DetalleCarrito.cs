using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class DetalleCarrito
{
    [Key]
    public int id_detalle_carrito { get; set; }
    public int? id_carrito { get; set; }
    public int? id_producto { get; set; }
    public int cantidad { get; set; }

    [ForeignKey("id_carrito")]
    public Carrito Carrito { get; set; }
    [ForeignKey("id_producto")]
    public Producto Producto { get; set; }
}
