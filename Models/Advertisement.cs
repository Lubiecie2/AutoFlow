using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoFlow.Models
{
    public class Advertisement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = null!; 

        [Required]
        [MaxLength(100)]
        public string Model { get; set; } = null!; 

        [Required]
        public int Year { get; set; } 

        [Required]
        [MaxLength(50)]
        public string Color { get; set; } = null!; 

        [Required]
        public int Mileage { get; set; } 

        [Required]
        [MaxLength(100)]
        public string Engine { get; set; } = null!; 

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } 

        [MaxLength(1000)]
        public string? Description { get; set; } 

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }
    }
}
