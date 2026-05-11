using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class Pedido
{
    [Key]
    public int id_pedido { get; set; }
    public int? id_usuario { get; set; }
    public int? id_direccion { get; set; }
    public DateTime fecha_pedido { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "decimal(10,2)")]
    public decimal? total { get; set; }
    public int? id_estado_pedido { get; set; }

    [ForeignKey("id_usuario")]
    public Usuario Usuario { get; set; }
    [ForeignKey("id_direccion")]
    public DireccionUsuario? DireccionUsuario { get; set; }
    [ForeignKey("id_estado_pedido")]
    public EstadoPedido EstadoPedido { get; set; }

    public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    public Envio? Envio { get; set; }
    public Factura? Factura { get; set; }
}
