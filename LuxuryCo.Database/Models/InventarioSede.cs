using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class InventarioSede
{
    [Key]
    public int id_inventario { get; set; }
    
    public int id_producto { get; set; }
    
    public int id_sede { get; set; }

    public int cantidad_disponible { get; set; } = 0;
    
    public int umbral_minimo { get; set; } = 5;

    [ForeignKey("id_producto")]
    public Producto Producto { get; set; }

    [ForeignKey("id_sede")]
    public Sede Sede { get; set; }
}
