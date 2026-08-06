using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class HistorialChatAi
{
    [Key]
    public int id_historial { get; set; }
    
    public int? id_usuario { get; set; } // Opcional: si el usuario inició sesión
    
    [MaxLength(255)]
    public string session_id { get; set; } = string.Empty; // Cookie ID para usuarios anónimos
    
    [Required]
    [MaxLength(20)]
    public string role { get; set; } = string.Empty; // "user" o "assistant"
    
    [Required]
    public string content { get; set; } = string.Empty; // El texto del mensaje
    
    public DateTime fecha_creacion { get; set; } = DateTime.UtcNow;

    [ForeignKey("id_usuario")]
    public Usuario? Usuario { get; set; }
}
