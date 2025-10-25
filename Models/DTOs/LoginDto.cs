using System.ComponentModel.DataAnnotations;

namespace AutoFlow.Models.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Hasło jest wymagane")]
        public string Password { get; set; } = null!;
    }
}