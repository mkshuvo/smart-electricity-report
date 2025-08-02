using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace desco_report_server.Models;

public class DescoRechargeHistory
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int DescoAccountId { get; set; }
    
    [Required]
    public DateTime RechargeDate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }
    
    [StringLength(50)]
    public string? TransactionId { get; set; }
    
    [StringLength(50)]
    public string? PaymentMethod { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [StringLength(20)]
    public string? Status { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
}