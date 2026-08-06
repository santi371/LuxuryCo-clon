using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Database.Models;

public class MetodoPago
{
    [Key]
    public int id_metodo_pago { get; set; }
    [MaxLength(50)]
    public string nombre { get; set; }
    [MaxLength(150)]
    public string descripcion { get; set; }

    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
