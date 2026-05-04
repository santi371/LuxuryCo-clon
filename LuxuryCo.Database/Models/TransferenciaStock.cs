using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class TransferenciaStock
{
    [Key]
    public int id_transferencia { get; set; }
    
    public int id_producto { get; set; }
    
    public int? id_sede_origen { get; set; }
    
    public int? id_sede_destino { get; set; }
    
    public int cantidad { get; set; }
    
    public int? id_usuario_solicita { get; set; }

    [MaxLength(20)]
    public string estado { get; set; } = "PENDIENTE";
    
    public DateTime fecha_solicitud { get; set; } = DateTime.UtcNow;
    
    public DateTime? fecha_completada { get; set; }

    [ForeignKey("id_producto")]
    public Producto Producto { get; set; }

    [ForeignKey("id_sede_origen")]
    public Sede? SedeOrigen { get; set; }

    [ForeignKey("id_sede_destino")]
    public Sede? SedeDestino { get; set; }

    [ForeignKey("id_usuario_solicita")]
    public Usuario? UsuarioSolicita { get; set; }
}
