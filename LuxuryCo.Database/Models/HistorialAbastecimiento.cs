using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class HistorialAbastecimiento
{
    [Key]
    public int id_abastecimiento { get; set; }
    
    public int id_producto { get; set; }
    
    public int? id_proveedor { get; set; }
    
    public int id_sede { get; set; }
    
    public int? id_usuario_registra { get; set; }

    public int cantidad { get; set; }
    
    public decimal costo_unitario { get; set; }
    
    public DateTime fecha_ingreso { get; set; } = DateTime.UtcNow;
    
    public string? notas { get; set; }

    [ForeignKey("id_producto")]
    public Producto Producto { get; set; }

    [ForeignKey("id_proveedor")]
    public Proveedor? Proveedor { get; set; }

    [ForeignKey("id_sede")]
    public Sede Sede { get; set; }

    [ForeignKey("id_usuario_registra")]
    public Usuario? UsuarioRegistra { get; set; }
}
