using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class DireccionUsuario
{
    [Key]
    public int id_direccion { get; set; }
    public int? id_usuario { get; set; }
    [Required]
    [MaxLength(200)]
    public string direccion { get; set; }
    [MaxLength(200)]
    public string? referencia { get; set; }
    public int? id_ciudad { get; set; }

    [ForeignKey("id_usuario")]
    public Usuario Usuario { get; set; }
    [ForeignKey("id_ciudad")]
    public Ciudad Ciudad { get; set; }
}
