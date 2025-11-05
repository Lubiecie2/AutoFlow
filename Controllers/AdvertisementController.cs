using AutoFlow.Data;
using AutoFlow.Models;
using AutoFlow.Models.DTOs;
using AutoFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow.Controllers
{
    public class AdvertisementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public AdvertisementController(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        /****************************************************
         * GET: /Advertisement/Create
         * Wyświetla formularz tworzenia ogłoszenia
         ****************************************************/
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        /****************************************************
         * POST: /Advertisement/Create
         * Tworzy nowe ogłoszenie
         ****************************************************/
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAdvertisementDto model)
        {
            try
            {
                var userId = await GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { success = false, message = "Musisz być zalogowany" });

                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Nieprawidłowe dane" });

                var advertisement = new Advertisement
                {
                    UserId = userId.Value,
                    Brand = model.Brand,
                    Model = model.Model,
                    Year = model.Year,
                    Color = model.Color,
                    Mileage = model.Mileage,
                    Engine = model.Engine,
                    Price = model.Price,
                    Description = model.Description,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Advertisements.Add(advertisement);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Ogłoszenie zostało dodane i oczekuje na weryfikację", advertisementId = advertisement.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd serwera: {ex.Message}" });
            }
        }

        /****************************************************
         * GET: /Advertisement/GetAll
         * Pobiera wszystkie zatwierdzone ogłoszenia
         ****************************************************/
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var advertisements = await _context.Advertisements
                    .Include(a => a.User)
                    .Where(a => a.Status == "Approved")
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
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd serwera: {ex.Message}" });
            }
        }

        /****************************************************
         * GET: /Advertisement/MyAdvertisements
         * Pobiera ogłoszenia zalogowanego użytkownika
         ****************************************************/
        [HttpGet]
        public async Task<IActionResult> MyAdvertisements()
        {
            try
            {
                var userId = await GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { success = false, message = "Musisz być zalogowany" });

                var advertisements = await _context.Advertisements
                    .Where(a => a.UserId == userId.Value)
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
                        a.Status,
                        a.CreatedAt
                    })
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return Ok(new { success = true, advertisements });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd serwera: {ex.Message}" });
            }
        }

        private async Task<int?> GetCurrentUserId()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return null;

            return _jwtService.ValidateToken(token);
        }
        /****************************************************
         * GET: /Advertisement/Details/{id}
         * Wyświetla szczegóły ogłoszenia
         ****************************************************/
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Id == id && a.Status == "Approved");

                if (advertisement == null)
                    return NotFound();

                return View(advertisement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Błąd serwera");
            }
        }
    }
}
