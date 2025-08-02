using System.ComponentModel.DataAnnotations;

namespace desco_report_server.Models
{
    public class Bill
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int AccountId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string BillNumber { get; set; } = null!;
        
        [Required]
        public DateTime BillPeriodStart { get; set; }
        
        [Required]
        public DateTime BillPeriodEnd { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal BillAmount { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal DueAmount { get; set; }
        
        [Required]
        public DateTime DueDate { get; set; }
        
        public DateTime? PaymentDate { get; set; }
        
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Unpaid";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        

    }
}