using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Data;
using Staybnb.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Staybnb.Controllers
{
    public class PropertyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;



        public PropertyController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public IActionResult Index(
            string searchString,
            decimal? maxPrice,
            int? guests,
            string sortOrder)
        {
            var properties = _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                properties = properties.Where(p =>
                    p.Title.Contains(searchString)
                    || p.Address.Contains(searchString));
            }

            if (maxPrice.HasValue)
            {
                properties = properties.Where(p =>
                    p.PricePerNight <= maxPrice.Value);
            }

            if (guests.HasValue)
            {
                properties = properties.Where(p =>
                    p.MaxGuests >= guests.Value);
            }

            ViewBag.SearchString = searchString;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Guests = guests;
            ViewBag.SortOrder = sortOrder;

            switch (sortOrder)
            {
                case "price_asc":
                    properties = properties
                        .OrderBy(p => p.PricePerNight);
                    break;

                case "price_desc":
                    properties = properties
                        .OrderByDescending(p => p.PricePerNight);
                    break;

                default:
                    properties = properties
                        .OrderByDescending(p => p.Id);
                    break;
            }

            return View(properties.ToList()); 
        }

        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        public IActionResult Create()
        {
            var property = new Property
            {
                IsActive = false
            };
            return View(property);
        }

        [HttpPost]
        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        public async Task<IActionResult> Create(
        Property property,
        List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                property.HostId = user.Id;

                property.IsActive = false;

                _context.Properties.Add(property);

                await _context.SaveChangesAsync();

                if (imageFiles != null && imageFiles.Count > 0)
                {
                    string uploadsFolder =
                        Path.Combine(
                            _environment.WebRootPath,
                            "images/properties");

                    Directory.CreateDirectory(uploadsFolder);

                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName =
                                Guid.NewGuid().ToString()
                                + "_"
                                + file.FileName;

                            string filePath =
                                Path.Combine(
                                    uploadsFolder,
                                    uniqueFileName);

                            using (var stream =
                                new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var propertyImage = new PropertyImage
                            {
                                PropertyId = property.Id,
                                ImageUrl =
                                    "/images/properties/" + uniqueFileName
                            };

                            _context.PropertyImages.Add(propertyImage);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(property);
        }


        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Approve(int id)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property == null)
            {
                return NotFound();
            }

            property.IsActive = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        public async Task<IActionResult> MyProperties()
        {
            var user = await _userManager.GetUserAsync(User);

            var properties = await _context.Properties
                .Include(p => p.Images)
                .Where(p => p.HostId == user.Id)
                .ToListAsync();

            return View(properties);
        }
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> PendingProperties()
        {
            var properties = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Host)
                .Where(p => !p.IsActive)
                .ToListAsync();

            return View(properties);
        }
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            foreach (var image in property.Images)
            {
                _context.PropertyImages.Remove(image);
            }

            _context.Properties.Remove(property);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingProperties));
        }
        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        public async Task<IActionResult> Edit(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            bool canEdit =
                User.IsInRole("Admin")
                || User.IsInRole("SuperAdmin")
                || property.HostId == user.Id;

            if (!canEdit)
            {
                return Forbid();
            }

            return View(property);
        }
        [HttpPost]
        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        public async Task<IActionResult> Edit(
    Property property,
    List<IFormFile> imageFiles)
        {
            var existingProperty = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == property.Id);

            if (existingProperty == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            bool canEdit =
                User.IsInRole("Admin")
                || User.IsInRole("SuperAdmin")
                || existingProperty.HostId == user.Id;

            if (!canEdit)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                existingProperty.Title = property.Title;
                existingProperty.Description = property.Description;
                existingProperty.PricePerNight = property.PricePerNight;
                existingProperty.Address = property.Address;

                if (imageFiles != null && imageFiles.Count > 0)
                {
                    string uploadsFolder =
                        Path.Combine(
                            _environment.WebRootPath,
                            "images/properties");

                    Directory.CreateDirectory(uploadsFolder);

                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName =
                                Guid.NewGuid().ToString()
                                + "_"
                                + file.FileName;

                            string filePath =
                                Path.Combine(
                                    uploadsFolder,
                                    uniqueFileName);

                            using (var stream =
                                new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var propertyImage = new PropertyImage
                            {
                                PropertyId = existingProperty.Id,
                                ImageUrl =
                                    "/images/properties/" + uniqueFileName
                            };

                            _context.PropertyImages.Add(propertyImage);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyProperties));
            }

            return View(property);
        }
        [HttpPost]
        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        public async Task<IActionResult> Reject(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Bookings)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            bool canDelete =
                User.IsInRole("Admin")
                || User.IsInRole("SuperAdmin")
                || property.HostId == user.Id;

            if (!canDelete)
            {
                return Forbid();
            }

            bool hasActiveBookings =
                property.Bookings.Any(b =>
                    b.Status != "Cancelled"
                    && b.CheckOutDate >= DateTime.Today);

            if (hasActiveBookings)
            {
                TempData["Error"] =
                    "Cannot delete a property with active bookings.";

                return RedirectToAction(nameof(MyProperties));
            }

            foreach (var image in property.Images)
            {
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    string imagePath =
                        Path.Combine(
                            _environment.WebRootPath,
                            image.ImageUrl.TrimStart('/'));

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.PropertyImages.Remove(image);
            }

            _context.Properties.Remove(property);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Property deleted successfully.";

            return RedirectToAction(nameof(MyProperties));
        }

        public async Task<IActionResult> Details(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.Guest)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (property == null)
            {
                return NotFound();
            }

            if (!property.IsActive)
            {
                var user = await _userManager.GetUserAsync(User);

                bool canAccess =
                    User.IsInRole("Admin")
                    || User.IsInRole("SuperAdmin")
                    || (user != null
                        && property.HostId == user.Id);

                if (!canAccess)
                {
                    return NotFound();
                }
            }

            return View(property);
        }
    }
}