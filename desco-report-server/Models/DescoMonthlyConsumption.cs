using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace desco_report_server.Models;

public class DescoMonthlyConsumption
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int DescoAccountId { get; set; }
    
    [Required]
    public int Year { get; set; }
    
    [Required]
    public int Month { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal ConsumptionValue { get; set; }
    
    [Required]
    [StringLength(10)]
    public string Unit { get; set; } = "kWh";
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? Cost { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? AverageDailyConsumption { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
}