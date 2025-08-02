using System.ComponentModel.DataAnnotations;

namespace desco_report_server.Models
{
    public class DailyConsumption
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int AccountId { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal ConsumptionValue { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Unit { get; set; } = "kWh";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        

    }
}