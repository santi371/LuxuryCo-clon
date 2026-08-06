using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Database.Models;

public class EstadoPedido
{
    [Key]
    public int id_estado_pedido { get; set; }
    [MaxLength(50)]
    public string nombre_estado { get; set; }

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
