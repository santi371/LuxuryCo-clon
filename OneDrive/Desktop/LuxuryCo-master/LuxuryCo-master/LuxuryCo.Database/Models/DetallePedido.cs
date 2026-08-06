using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class DetallePedido
{
    [Key]
    public int id_detalle_pedido { get; set; }
    public int? id_pedido { get; set; }
    public int? id_producto { get; set; }
    public int? cantidad { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal? precio_unitario { get; set; }

    [ForeignKey("id_pedido")]
    public Pedido Pedido { get; set; }
    [ForeignKey("id_producto")]
    public Producto Producto { get; set; }
}
