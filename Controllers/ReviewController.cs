using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;

namespace Staybnb.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            int propertyId,
            int rating,
            string comment)
        {
            var user = await _userManager.GetUserAsync(User);

            bool hasBooked = await _context.Bookings
                .AnyAsync(b =>
                    b.PropertyId == propertyId
                    && b.GuestId == user.Id
                    && b.Status == "Confirmed"
                    && b.CheckOutDate < DateTime.Today);

            if (!hasBooked)
            {
                return BadRequest();
            }

            bool alreadyReviewed = await _context.Reviews
                .AnyAsync(r =>
                    r.PropertyId == propertyId
                    && r.GuestId == user.Id);

            if (alreadyReviewed)
            {
                return BadRequest();
            }

            var review = new Review
            {
                PropertyId = propertyId,
                GuestId = user.Id,
                Rating = rating,
                Comment = comment
            };

            _context.Reviews.Add(review);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "Property",
                new { id = propertyId });
        }
    }
}