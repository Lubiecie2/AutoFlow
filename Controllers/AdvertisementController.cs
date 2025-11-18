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
        private readonly IWebHostEnvironment _environment;

        public AdvertisementController(ApplicationDbContext context, IJwtService jwtService, IWebHostEnvironment environment)
        {
            _context = context;
            _jwtService = jwtService;
            _environment = environment;
        }

        /****************************************************
         * GET: /Advertisement/Create
         * Strona dodawania nowego ogłoszenia
         ****************************************************/

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var userId = _jwtService.ValidateToken(token);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        /****************************************************
         * POST: /Advertisement/CreateWithImages
         * Tworzy nowe ogłoszenie z przesłanymi zdjęciami
         ****************************************************/

        [HttpPost]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> CreateWithImages()
        {
            try
            {
                var userId = await GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { success = false, message = "Musisz być zalogowany" });

                var form = await Request.ReadFormAsync();

                var brand = form["Brand"].ToString().Trim();
                var model = form["Model"].ToString().Trim();
                var yearStr = form["Year"].ToString();
                var color = form["Color"].ToString().Trim();
                var mileageStr = form["Mileage"].ToString();
                var engine = form["Engine"].ToString().Trim();
                var priceStr = form["Price"].ToString();
                var description = form["Description"].ToString().Trim();
                var mainImageIndexStr = form["MainImageIndex"].ToString();
                var images = form.Files.GetFiles("Images");

                if (string.IsNullOrEmpty(brand) || brand.Length > 100)
                    return BadRequest(new { success = false, message = "Marka jest wymagana (max 100 znaków)" });

                if (string.IsNullOrEmpty(model) || model.Length > 100)
                    return BadRequest(new { success = false, message = "Model jest wymagany (max 100 znaków)" });

                if (!int.TryParse(yearStr, out int year) || year < 1900 || year > 2100)
                    return BadRequest(new { success = false, message = "Rocznik musi być między 1900 a 2100" });

                if (string.IsNullOrEmpty(color) || color.Length > 50)
                    return BadRequest(new { success = false, message = "Kolor jest wymagany (max 50 znaków)" });

                if (!int.TryParse(mileageStr, out int mileage) || mileage < 0 || mileage > 999999999)
                    return BadRequest(new { success = false, message = "Przebieg musi być między 0 a 999999999" });

                if (string.IsNullOrEmpty(engine) || engine.Length > 100)
                    return BadRequest(new { success = false, message = "Silnik jest wymagany (max 100 znaków)" });

                if (!decimal.TryParse(priceStr, out decimal price) || price < 0.01m)
                    return BadRequest(new { success = false, message = "Cena musi być większa niż 0" });

                if (description.Length > 1000)
                    return BadRequest(new { success = false, message = "Opis może mieć maksymalnie 1000 znaków" });

                if (images.Count == 0)
                    return BadRequest(new { success = false, message = "Musisz dodać co najmniej 1 zdjęcie" });

                if (images.Count > 10)
                    return BadRequest(new { success = false, message = "Maksymalnie można dodać 10 zdjęć" });

                if (!int.TryParse(mainImageIndexStr, out int mainImageIndex) || mainImageIndex < 0 || mainImageIndex >= images.Count)
                    mainImageIndex = 0;

                var advertisement = new Advertisement
                {
                    UserId = userId.Value,
                    Brand = brand,
                    Model = model,
                    Year = year,
                    Color = color,
                    Mileage = mileage,
                    Engine = engine,
                    Price = price,
                    Description = string.IsNullOrEmpty(description) ? null : description,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Advertisements.Add(advertisement);
                await _context.SaveChangesAsync();

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "advertisements", advertisement.Id.ToString());
                Directory.CreateDirectory(uploadsFolder);

                for (int i = 0; i < images.Count; i++)
                {
                    var image = images[i];
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var advertisementImage = new AdvertisementImage
                    {
                        AdvertisementId = advertisement.Id,
                        ImagePath = $"/uploads/advertisements/{advertisement.Id}/{fileName}",
                        DisplayOrder = i,
                        IsMainImage = (i == mainImageIndex)
                    };

                    _context.AdvertisementImages.Add(advertisementImage);
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Ogłoszenie zostało dodane i oczekuje na weryfikację" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd serwera: {ex.Message}" });
            }
        }

        /****************************************************
         * GET: /Advertisement/GetAll
         * API - Pobiera wszystkie zatwierdzone ogłoszenia
         ****************************************************/

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var advertisements = await _context.Advertisements
                    .Include(a => a.User)
                    .Include(a => a.Images)
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
                        Username = a.User.Username,
                        MainImage = a.Images.FirstOrDefault(i => i.IsMainImage) != null
                            ? a.Images.FirstOrDefault(i => i.IsMainImage)!.ImagePath
                            : a.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault() != null
                                ? a.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()!.ImagePath
                                : null,
                        ImageCount = a.Images.Count
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
         * GET: /Advertisement/Details/{id}
         * Strona szczegółów ogłoszenia (tylko Approved lub właściciel)
         ****************************************************/
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = await GetCurrentUserId();

                var advertisement = await _context.Advertisements
                    .Include(a => a.User)
                    .Include(a => a.Images)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (advertisement == null)
                    return NotFound();

                if (advertisement.Status != "Approved" && advertisement.UserId != userId)
                    return NotFound();

                return View(advertisement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Błąd serwera");
            }
        }

        /****************************************************
         * GET: /Advertisement/AdminPreview/{id}
         * Podgląd ogłoszenia dla administratora (wszystkie statusy)
         ****************************************************/

        [HttpGet]
        public async Task<IActionResult> AdminPreview(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var userId = _jwtService.ValidateToken(token);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user?.Role != "Admin")
                return Forbid();

            try
            {
                var advertisement = await _context.Advertisements
                    .Include(a => a.User)
                    .Include(a => a.Images)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (advertisement == null)
                    return NotFound();

                return View("Details", advertisement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Błąd serwera");
            }
        }

        /****************************************************
         * GET: /Advertisement/MyAdvertisements
         * API - Pobiera ogłoszenia zalogowanego użytkownika
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
                    .Include(a => a.Images)
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
                        a.CreatedAt,
                        MainImage = a.Images.FirstOrDefault(i => i.IsMainImage) != null
                            ? a.Images.FirstOrDefault(i => i.IsMainImage)!.ImagePath
                            : a.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault() != null
                                ? a.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()!.ImagePath
                                : null
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
         * GET: /Advertisement/Edit/{id}
         * Strona edycji ogłoszenia (tylko dla twórcy)
         ****************************************************/

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var advertisement = await _context.Advertisements
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId.Value);

            if (advertisement == null)
                return NotFound();

            return View(advertisement);
        }

        /****************************************************
          * PUT: /Advertisement/Update/{id}
          * Aktualizuje ogłoszenie (tylko dla twórcy)
          ****************************************************/

        [HttpPut]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                var userId = await GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { success = false, message = "Musisz być zalogowany" });

                var advertisement = await _context.Advertisements
                    .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId.Value);

                if (advertisement == null)
                    return NotFound(new { success = false, message = "Ogłoszenie nie istnieje lub nie masz do niego dostępu" });

                var form = await Request.ReadFormAsync();

                var brand = form["Brand"].ToString().Trim();
                var model = form["Model"].ToString().Trim();
                var yearStr = form["Year"].ToString();
                var color = form["Color"].ToString().Trim();
                var mileageStr = form["Mileage"].ToString();
                var engine = form["Engine"].ToString().Trim();
                var priceStr = form["Price"].ToString();
                var description = form["Description"].ToString().Trim();

                if (string.IsNullOrEmpty(brand) || brand.Length > 100)
                    return BadRequest(new { success = false, message = "Marka jest wymagana (max 100 znaków)" });

                if (string.IsNullOrEmpty(model) || model.Length > 100)
                    return BadRequest(new { success = false, message = "Model jest wymagany (max 100 znaków)" });

                if (!int.TryParse(yearStr, out int year) || year < 1900 || year > 2100)
                    return BadRequest(new { success = false, message = "Rocznik musi być między 1900 a 2100" });

                if (string.IsNullOrEmpty(color) || color.Length > 50)
                    return BadRequest(new { success = false, message = "Kolor jest wymagany (max 50 znaków)" });

                if (!int.TryParse(mileageStr, out int mileage) || mileage < 0 || mileage > 999999999)
                    return BadRequest(new { success = false, message = "Przebieg musi być między 0 a 999999999" });

                if (string.IsNullOrEmpty(engine) || engine.Length > 100)
                    return BadRequest(new { success = false, message = "Silnik jest wymagany (max 100 znaków)" });

                if (!decimal.TryParse(priceStr, out decimal price) || price < 0.01m)
                    return BadRequest(new { success = false, message = "Cena musi być większa niż 0" });

                if (description.Length > 1000)
                    return BadRequest(new { success = false, message = "Opis może mieć maksymalnie 1000 znaków" });

                advertisement.Brand = brand;
                advertisement.Model = model;
                advertisement.Year = year;
                advertisement.Color = color;
                advertisement.Mileage = mileage;
                advertisement.Engine = engine;
                advertisement.Price = price;
                advertisement.Description = description;
                advertisement.Status = "Pending";
                advertisement.ApprovedAt = null;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Ogłoszenie zostało zaktualizowane i oczekuje na weryfikację" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd serwera: {ex.Message}" });
            }
        }

        /****************************************************
         * DELETE: /Advertisement/Delete/{id}
         * Usuwa ogłoszenie (twórca lub admin)
         ****************************************************/

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = await GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { success = false, message = "Musisz być zalogowany" });

                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                    return Unauthorized(new { success = false, message = "Użytkownik nie istnieje" });

                var advertisement = await _context.Advertisements
                    .Include(a => a.Images)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (advertisement == null)
                    return NotFound(new { success = false, message = "Ogłoszenie nie istnieje" });

                if (advertisement.UserId != userId.Value && user.Role != "Admin")
                    return Forbid();

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "advertisements", advertisement.Id.ToString());
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

        /****************************************************
         * Metoda pomocnicza - pobiera ID zalogowanego użytkownika
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