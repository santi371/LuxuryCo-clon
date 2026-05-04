using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Database.Models;

public class EstadoEnvio
{
    [Key]
    public int id_estado_envio { get; set; }
    [MaxLength(50)]
    public string nombre_estado { get; set; }

    public ICollection<Envio> Envios { get; set; } = new List<Envio>();
}
