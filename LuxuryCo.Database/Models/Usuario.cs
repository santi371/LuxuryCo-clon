using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class Usuario
{
    [Key]
    public int id_usuario { get; set; }
    [Required]
    [MaxLength(100)]
    public string nombre { get; set; }
    [Required]
    [MaxLength(100)]
    public string apellido { get; set; }
    [Required]
    [MaxLength(150)]
    public string email { get; set; }
    [Required]
    [MaxLength(255)]
    public string password_hash { get; set; }
    [MaxLength(20)]
    public string? telefono { get; set; }
    public DateTime fecha_registro { get; set; } = DateTime.UtcNow;
    public bool activo { get; set; } = true;
    public int? id_rol { get; set; }
    public int? id_sede { get; set; }

    public string? foto_perfil_url { get; set; }
    public bool two_factor_enabled { get; set; }
    public string? two_factor_secret { get; set; }
    
    [MaxLength(6)]
    public string? reset_password_code { get; set; }
    public DateTime? reset_password_expires { get; set; }

    [ForeignKey("id_rol")]
    public Rol Rol { get; set; }

    [ForeignKey("id_sede")]
    public Sede? Sede { get; set; }

    public ICollection<DireccionUsuario> Direcciones { get; set; } = new List<DireccionUsuario>();
    public ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    public ICollection<Resena> Resenas { get; set; } = new List<Resena>();
}
