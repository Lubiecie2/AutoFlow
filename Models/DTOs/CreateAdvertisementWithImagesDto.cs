using System.ComponentModel.DataAnnotations;

namespace AutoFlow.Models.DTOs
{
    public class CreateAdvertisementWithImagesDto
    {
        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Model { get; set; } = null!;

        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required]
        [MaxLength(50)]
        public string Color { get; set; } = null!;

        [Required]
        [Range(0, 999999999)]
        public int Mileage { get; set; }

        [Required]
        [MaxLength(100)]
        public string Engine { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
