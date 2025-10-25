namespace AutoFlow.Models.DTOs
{
    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}