using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class Factura
{
    [Key]
    public int id_factura { get; set; }
    public int? id_pedido { get; set; }
    public DateTime fecha_factura { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "decimal(10,2)")]
    public decimal? total { get; set; }
    public int? id_metodo_pago { get; set; }

    [ForeignKey("id_pedido")]
    public Pedido Pedido { get; set; }
    [ForeignKey("id_metodo_pago")]
    public MetodoPago MetodoPago { get; set; }
}
