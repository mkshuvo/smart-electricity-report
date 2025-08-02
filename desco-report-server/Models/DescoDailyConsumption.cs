using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace desco_report_server.Models;

public class DescoDailyConsumption
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int DescoAccountId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal ConsumptionValue { get; set; }
    
    [Required]
    [StringLength(10)]
    public string Unit { get; set; } = "kWh";
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? Cost { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
}