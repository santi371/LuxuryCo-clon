using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

[Table("proveedor")]
public class Proveedor
{
    [Key]
    [Column("id_proveedor")]
    public int id_proveedor { get; set; }

    [Column("nombre")]
    [Required]
    [MaxLength(150)]
    public string nombre { get; set; }

    [Column("contacto")]
    [MaxLength(100)]
    public string? contacto { get; set; }

    [Column("telefono")]
    [MaxLength(50)]
    public string? telefono { get; set; }

    [Column("email")]
    [MaxLength(150)]
    public string? email { get; set; }

    [Column("activo")]
    public bool activo { get; set; } = true;

    // Relaciones (Opcionales para navegación)
    // public ICollection<ProductoProveedor> ProductoProveedores { get; set; }
}
