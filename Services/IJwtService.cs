using AutoFlow.Models;

namespace AutoFlow.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        int? ValidateToken(string token);
    }
}