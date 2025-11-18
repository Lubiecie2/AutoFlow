using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoFlow.Data;
using AutoFlow.Models;
using AutoFlow.Models.DTOs;
using AutoFlow.Services;

namespace AutoFlow.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public AdminController(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            if (!await IsAdmin())
                return Unauthorized(new { success = false, message = "Brak uprawnień" });

            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Role,
                    u.CreatedAt
                })
                .OrderBy(u => u.Username)
                .ToListAsync();

            return Ok(new { success = true, users });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleDto model)
        {
            if (!await IsAdmin())
                return Unauthorized(new { success = false, message = "Brak uprawnień" });

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Nieprawidłowe dane" });

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
                return NotFound(new { success = false, message = "Użytkownik nie istnieje" });

            user.Role = model.NewRole;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Rola użytkownika została zaktualizowana" });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!await IsAdmin())
                return Unauthorized(new { success = false, message = "Brak uprawnień" });

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "Użytkownik nie istnieje" });

            var currentUserId = await GetCurrentUserId();
            if (currentUserId == id)
                return BadRequest(new { success = false, message = "Nie możesz usunąć własnego konta" });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Użytkownik został usunięty" });
        }

        private async Task<bool> IsAdmin()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return false;

            var userId = _jwtService.ValidateToken(token);
            if (userId == null)
                return false;

            var user = await _context.Users.FindAsync(userId.Value);
            return user?.Role == "Admin";
        }

        private async Task<int?> GetCurrentUserId()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return null;

            return _jwtService.ValidateToken(token);
        }

        [HttpGet]
        public async Task<IActionResult> Advertisements()
        {
            if (!await IsAdmin())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingAdvertisements()
        {
            if (!await IsAdmin())
                return Unauthorized(new { success = false, message = "Brak uprawnień" });

            var advertisements = await _context.Advertisements
                .Include(a => a.User)
                .Where(a => a.Status == "Pending")
                .Select(a => new
                {
                    a.Id,
                    a.Brand,
                    a.Model,
                    a.Year,
                    a.Color,
                    a.Mileage,
                    a.Engine,
                    a.Price,
                    a.Description,
                    a.CreatedAt,
                    Username = a.User.Username
                })
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(new { success = true, advertisements });
        }

        [HttpPut]
        public async Task<IActionResult> ApproveAdvertisement(int id)
        {
            if (!await IsAdmin())
                return Unauthorized(new { success = false, message = "Brak uprawnień" });

            var advertisement = await _context.Advertisements.FindAsync(id);

            if (advertisement == null)
                return NotFound(new { success = false, message = "Ogłoszenie nie istnieje" });

            advertisement.Status = "Approved";
            advertisement.ApprovedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Ogłoszenie zostało zatwierdzone" });
        }

        [HttpPut]
        public async Task<IActionResult> RejectAdvertisement(int id)
        {
            if (!await IsAdmin())
                return Unauthorized(new { success = false, message = "Brak uprawnień" });

            var advertisement = await _context.Advertisements.FindAsync(id);

            if (advertisement == null)
                return NotFound(new { success = false, message = "Ogłoszenie nie istnieje" });

            advertisement.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Ogłoszenie zostało odrzucone" });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAdvertisement(int id)
        {
            if (!await IsAdmin())
                return Unauthorized(new { success = false, message = "Brak uprawnień" });

            try
            {
                var advertisement = await _context.Advertisements
                    .Include(a => a.Images)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (advertisement == null)
                    return NotFound(new { success = false, message = "Ogłoszenie nie istnieje" });

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "advertisements", advertisement.Id.ToString());
                if (Directory.Exists(uploadsFolder))
                {
                    Directory.Delete(uploadsFolder, true);
                }

                _context.Advertisements.Remove(advertisement);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Ogłoszenie zostało usunięte" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd serwera: {ex.Message}" });
            }
        }
    }
}