using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;

namespace Staybnb.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create(int propertyId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == propertyId && p.IsActive);

            if (property == null)
            {
                return NotFound();
            }

            var booking = new Booking
            {
                PropertyId = property.Id,
                Property = property,
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(1),
                Guests = 1,
                Status = "Pending"
            };

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Booking booking)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == booking.PropertyId);

            if (property == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (property.HostId == user.Id)
            {
                ModelState.AddModelError("",
                    "You cannot book your own property.");
            }

            if (booking.CheckOutDate <= booking.CheckInDate)
            {
                ModelState.AddModelError("",
                    "Check-out date must be after check-in date.");
            }

            bool hasConflict = await _context.Bookings
                .AnyAsync(b =>
                    b.PropertyId == booking.PropertyId
                    &&
                    b.Status != "Cancelled"
                    &&
                    booking.CheckInDate < b.CheckOutDate
                    &&
                    booking.CheckOutDate > b.CheckInDate);

            if (hasConflict)
            {
                ModelState.AddModelError("",
                    "The property is already booked for the selected dates.");
            }
            if (ModelState.IsValid)
            {

                booking.GuestId = user.Id;

                var nights = (booking.CheckOutDate - booking.CheckInDate).Days;

                booking.TotalPrice =
                    nights * property.PricePerNight;

                booking.Status = "Pending";

                _context.Bookings.Add(booking);

                await _context.SaveChangesAsync();

                var notification = new Notification
                {
                    UserId = property.HostId,
                    Message =
                        $"New booking request for {property.Title}"
                };

                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyBookings));
            }

            booking.Property = property;

            return View(booking);
        }
        [HttpPost]
        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        public async Task<IActionResult> Approve(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b =>
                    b.Id == id
                    && b.Property.HostId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = "Confirmed";

            await _context.SaveChangesAsync();

            var notification = new Notification
            {
                UserId = booking.GuestId,
                Message =
                    $"Your booking for {booking.Property.Title} was approved."
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(HostBookings));
        }
        [HttpPost]
        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        public async Task<IActionResult> Reject(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b =>
                    b.Id == id
                    && b.Property.HostId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = "Rejected";

            await _context.SaveChangesAsync();

            var notification = new Notification
            {
                UserId = booking.GuestId,
                Message =
                    $"Your booking for {booking.Property.Title} was rejected."
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(HostBookings));
        }
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b =>
                    b.Id == id
                    && b.GuestId == user.Id);

            
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.CheckOutDate < DateTime.Today)
            {
                return BadRequest();
            }

            booking.Status = "Cancelled";

            await _context.SaveChangesAsync();



            return RedirectToAction(nameof(MyBookings));
        }

        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                .Where(b => b.GuestId == user.Id)
                .ToListAsync();

            return View(bookings);
        }

        [Authorize(Roles = "Host,Admin,SuperAdmin")]
        public async Task<IActionResult> HostBookings()
        {
            var user = await _userManager.GetUserAsync(User);

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                .Include(b => b.Guest)
                .Where(b => b.Property.HostId == user.Id)
                .ToListAsync();

            return View(bookings);
        }
    }
}