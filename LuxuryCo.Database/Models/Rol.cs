using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class Rol
{
    [Key]
    public int id_rol { get; set; }
    [Required]
    [MaxLength(50)]
    public string nombre_rol { get; set; }
    [MaxLength(150)]
    public string? descripcion { get; set; }

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
