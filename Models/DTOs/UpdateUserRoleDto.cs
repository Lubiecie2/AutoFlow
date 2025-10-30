using System.ComponentModel.DataAnnotations;

namespace AutoFlow.Models.DTOs
{
    public class UpdateUserRoleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [RegularExpression("^(User|Admin)$", ErrorMessage = "Rola musi być 'User' lub 'Admin'")]
        public string NewRole { get; set; } = null!;
    }
}
