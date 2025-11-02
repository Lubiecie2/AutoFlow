using AutoFlow.Data;
using AutoFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public ProfileController(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        /****************************************************
         * GET: /Profile/Index
         * Wyświetla profil zalogowanego użytkownika
         ****************************************************/
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return RedirectToAction("Login", "Account");

            return View(user);
        }

        /****************************************************
         * PRYWATNA METODA: GetCurrentUserId
         * Pobiera ID użytkownika z tokenu JWT
         ****************************************************/
        private async Task<int?> GetCurrentUserId()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return null;

            return _jwtService.ValidateToken(token);
        }
    }
}