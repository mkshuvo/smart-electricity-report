using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace desco_report_server.Models;

public class Account
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string AccountNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(100)]
    public string? Email { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal CurrentBalance { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalDue { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign key
    public string UserId { get; set; } = string.Empty;
    

}