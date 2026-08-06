using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class Resena
{
    [Key]
    public int id_resena { get; set; }
    public int? id_usuario { get; set; }
    public int? id_producto { get; set; }
    [Range(1, 5)]
    public int calificacion { get; set; }
    public string? comentario { get; set; }
    public string? nombre_invitado { get; set; }
    public DateTime fecha { get; set; } = DateTime.UtcNow;

    [ForeignKey("id_usuario")]
    public Usuario Usuario { get; set; }
    [ForeignKey("id_producto")]
    public Producto Producto { get; set; }
}
