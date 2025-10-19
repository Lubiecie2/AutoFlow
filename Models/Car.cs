using System.ComponentModel.DataAnnotations;

namespace AutoFlow.Models
{
    public class Car
    {
        public int Id { get; set; }

        [Required]
        public string Make { get; set; } = null!;

        [Required]
        public string Model { get; set; } = null!;

        [Required]
        public decimal Price { get; set; }
    }
}