using System.ComponentModel.DataAnnotations;

namespace AutoFlow.Models.DTOs
{
    public class CreateAdvertisementDto
    {
        [Required(ErrorMessage = "Marka jest wymagana")]
        [MaxLength(100)]
        public string Brand { get; set; } = null!;

        [Required(ErrorMessage = "Model jest wymagany")]
        [MaxLength(100)]
        public string Model { get; set; } = null!;

        [Required(ErrorMessage = "Rocznik jest wymagany")]
        [Range(1900, 2100, ErrorMessage = "Nieprawidłowy rok")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Kolor jest wymagany")]
        [MaxLength(50)]
        public string Color { get; set; } = null!;

        [Required(ErrorMessage = "Przebieg jest wymagany")]
        [Range(0, 999999999, ErrorMessage = "Nieprawidłowy przebieg")]
        public int Mileage { get; set; }

        [Required(ErrorMessage = "Silnik jest wymagany")]
        [MaxLength(100)]
        public string Engine { get; set; } = null!;

        [Required(ErrorMessage = "Cena jest wymagana")]
        [Range(0.01, 9999999.99, ErrorMessage = "Nieprawidłowa cena")]
        public decimal Price { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
