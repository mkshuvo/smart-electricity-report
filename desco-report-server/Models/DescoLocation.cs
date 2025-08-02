using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace desco_report_server.Models;

public class DescoLocation
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int DescoAccountId { get; set; }
    
    [StringLength(100)]
    public string? Division { get; set; }
    
    [StringLength(100)]
    public string? District { get; set; }
    
    [StringLength(100)]
    public string? Thana { get; set; }
    
    [StringLength(100)]
    public string? Area { get; set; }
    
    [StringLength(20)]
    public string? PostCode { get; set; }
    
    [StringLength(500)]
    public string? FullAddress { get; set; }
    
    [Column(TypeName = "decimal(10,6)")]
    public decimal? Latitude { get; set; }
    
    [Column(TypeName = "decimal(10,6)")]
    public decimal? Longitude { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
}