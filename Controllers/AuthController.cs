using AutoFlow.Data;
using AutoFlow.Models;
using AutoFlow.Models.DTOs;
using AutoFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace autoflow.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public AccountController(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Nieprawidłowe dane" });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                    return Ok(new { success = false, message = "Nieprawidłowa nazwa użytkownika lub hasło" });

                var token = _jwtService.GenerateToken(user);

                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60)
                });

                return Ok(new { 
                    success = true, 
                    message = "Zalogowano pomyślnie", 
                    username = user.Username, 
                    role = user.Role 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd serwera: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Nieprawidłowe dane" });

                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                    return Ok(new { success = false, message = "Nazwa użytkownika jest już zajęta" });

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = passwordHash,
                    Role = "User",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateToken(user);

                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60)
                });

                return Ok(new { 
                    success = true, 
                    message = "Rejestracja zakończona pomyślnie", 
                    username = user.Username, 
                    role = user.Role 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd serwera: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return Ok(new { success = true, message = "Wylogowano pomyślnie" });
        }

        [HttpGet]
        public async Task<IActionResult> CurrentUser()
        {
            try
            {
                var token = Request.Cookies["AuthToken"];
                
                if (string.IsNullOrEmpty(token))
                    return Ok(new { success = false, isAuthenticated = false });

                var userId = _jwtService.ValidateToken(token);
                
                if (userId == null)
                {
                    Response.Cookies.Delete("AuthToken");
                    return Ok(new { success = false, isAuthenticated = false });
                }

                var user = await _context.Users.FindAsync(userId.Value);
                
                if (user == null)
                    return Ok(new { success = false, isAuthenticated = false });

                return Ok(new 
                { 
                    success = true, 
                    isAuthenticated = true,
                    username = user.Username, 
                    role = user.Role 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd serwera: {ex.Message}" });
            }
        }
    }
}
