using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace desco_report_server.Models;

public class DescoRecentEvent
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int DescoAccountId { get; set; }
    
    [Required]
    public DateTime EventDate { get; set; }
    
    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? Category { get; set; }
    
    [StringLength(20)]
    public string? Priority { get; set; }
    
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
}