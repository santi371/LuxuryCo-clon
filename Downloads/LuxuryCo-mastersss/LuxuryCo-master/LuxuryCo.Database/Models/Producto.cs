using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class Producto
{
    [Key]
    public int id_producto { get; set; }
    [Required]
    [MaxLength(150)]
    public string nombre { get; set; }
    public string? descripcion { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal precio { get; set; }
    public int stock { get; set; } = 0;
    public bool activo { get; set; } = true;
    public DateTime fecha_creacion { get; set; } = DateTime.UtcNow;
    public int? id_categoria { get; set; }
    public int? id_marca { get; set; }
    [MaxLength(50)]
    public string? seccion { get; set; } // "Hombre", "Mujer", "Accesorios" — sección de la tienda pública

    [ForeignKey("id_categoria")]
    public Categoria Categoria { get; set; }
    [ForeignKey("id_marca")]
    public Marca Marca { get; set; }

    public ICollection<ImagenProducto> Imagenes { get; set; } = new List<ImagenProducto>();
    public ICollection<Resena> Resenas { get; set; } = new List<Resena>();
}
