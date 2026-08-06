using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class Ciudad
{
    [Key]
    public int id_ciudad { get; set; }
    [Required]
    [MaxLength(100)]
    public string nombre_ciudad { get; set; }
    public int? id_departamento { get; set; }

    [ForeignKey("id_departamento")]
    public Departamento Departamento { get; set; }
}
