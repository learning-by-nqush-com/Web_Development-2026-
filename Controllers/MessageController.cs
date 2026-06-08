using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;

namespace Staybnb.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Inbox()
        {
            var user = await _userManager.GetUserAsync(User);

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m =>
                    m.SenderId == user.Id
                    || m.ReceiverId == user.Id)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }

        public async Task<IActionResult> Create(string receiverId)
        {
            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == receiverId);

            if (receiver == null)
            {
                return NotFound();
            }

            ViewBag.ReceiverId = receiver.Id;
            ViewBag.ReceiverName = receiver.UserName;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            string receiverId,
            string content)
        {
            var sender = await _userManager.GetUserAsync(User);

            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == receiverId);

            if (receiver == null)
            {
                return NotFound();
            }

            var message = new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Content = content
            };

            _context.Messages.Add(message);

            var notification = new Notification
            {
                UserId = receiver.Id,
                Message =
                    $"New message from {sender.UserName}"
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Inbox));
        }
    }
}