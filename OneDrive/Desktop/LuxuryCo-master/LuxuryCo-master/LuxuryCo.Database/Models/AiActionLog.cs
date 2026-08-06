using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxuryCo.Database.Models;

public class AiActionLog
{
    [Key]
    public int Id { get; set; }
    
    public int? UserId { get; set; }
    
    [MaxLength(255)]
    public string SessionId { get; set; } = string.Empty;
    
    [Required]
    public string PromptOriginal { get; set; } = string.Empty;
    
    [Required]
    public string SanitizedPrompt { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string IntentDetected { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string ModelUsed { get; set; } = string.Empty;
    
    public double Confidence { get; set; }
    
    [MaxLength(50)]
    public string RiskLevel { get; set; } = "LOW";
    
    [MaxLength(255)]
    public string ActionExecuted { get; set; } = string.Empty;
    
    public string BeforeState { get; set; } = string.Empty; // JSON
    
    public string AfterState { get; set; } = string.Empty; // JSON
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(45)]
    public string IpAddress { get; set; } = string.Empty;
    
    public bool Success { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    // Enterprise Telemetry
    [MaxLength(100)]
    public string? TraceId { get; set; }

    [ForeignKey("UserId")]
    public Usuario? Usuario { get; set; }
}
