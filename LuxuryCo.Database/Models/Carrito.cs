using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class Carrito
{
    [Key]
    public int id_carrito { get; set; }
    public int? id_usuario { get; set; }
    public DateTime fecha_creacion { get; set; } = DateTime.UtcNow;

    [ForeignKey("id_usuario")]
    public Usuario Usuario { get; set; }
    public ICollection<DetalleCarrito> Detalles { get; set; } = new List<DetalleCarrito>();
}
