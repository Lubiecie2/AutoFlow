using System.ComponentModel.DataAnnotations;

namespace AutoFlow.Models.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
        [MinLength(3, ErrorMessage = "Nazwa użytkownika musi mieć minimum 3 znaki")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [MinLength(6, ErrorMessage = "Hasło musi mieć minimum 6 znaków")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane")]
        [Compare("Password", ErrorMessage = "Hasła nie są zgodne")]
        public string ConfirmPassword { get; set; } = null!;
    }
}