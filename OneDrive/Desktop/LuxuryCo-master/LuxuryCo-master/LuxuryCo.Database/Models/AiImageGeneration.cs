using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

[Table("ai_image_generation")]
public class AiImageGeneration
{
    [Key]
    public int Id { get; set; }
    
    public int? UserId { get; set; }
    
    [Required]
    public string PromptOriginal { get; set; } = string.Empty;
    
    public string OptimizedPrompt { get; set; } = string.Empty;
    
    public string NegativePrompt { get; set; } = string.Empty;
    
    public int Seed { get; set; }
    
    [MaxLength(100)]
    public string Provider { get; set; } = string.Empty;
    
    public double GenerationTimeMs { get; set; }
    
    [MaxLength(1000)]
    public string ImageUrl { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public Usuario? Usuario { get; set; }
}
