using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoFlow.Models
{
    public class AdvertisementImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AdvertisementId { get; set; }

        [ForeignKey("AdvertisementId")]
        public Advertisement Advertisement { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string ImagePath { get; set; } = null!;

        public bool IsMainImage { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
