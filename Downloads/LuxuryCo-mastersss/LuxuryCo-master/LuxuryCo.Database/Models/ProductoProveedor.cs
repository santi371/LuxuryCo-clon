using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class ProductoProveedor
{
    [Key, Column(Order = 0)]
    public int id_producto { get; set; }

    [Key, Column(Order = 1)]
    public int id_proveedor { get; set; }

    public decimal precio_costo { get; set; }
    
    public int? tiempo_entrega_dias { get; set; }

    [ForeignKey("id_producto")]
    public Producto Producto { get; set; }

    [ForeignKey("id_proveedor")]
    public Proveedor Proveedor { get; set; }
}
