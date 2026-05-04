using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Database.Models;

public class Departamento
{
    [Key]
    public int id_departamento { get; set; }
    [Required]
    [MaxLength(100)]
    public string nombre_departamento { get; set; }

    public ICollection<Ciudad> Ciudades { get; set; } = new List<Ciudad>();
}
