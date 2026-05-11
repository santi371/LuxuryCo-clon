using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class Envio
{
    [Key]
    public int id_envio { get; set; }
    public int? id_pedido { get; set; }
    public DateTime? fecha_envio { get; set; }
    [MaxLength(100)]
    public string? numero_guia { get; set; }
    public int? id_estado_envio { get; set; }

    [ForeignKey("id_pedido")]
    public Pedido Pedido { get; set; }
    [ForeignKey("id_estado_envio")]
    public EstadoEnvio EstadoEnvio { get; set; }
}
