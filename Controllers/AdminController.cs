using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Models;
using Staybnb.Data;
using Staybnb.ViewModels;

namespace Staybnb.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();

            var userList = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userList.Add(new UserRolesViewModel
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return View(userList);
        }


        public async Task<IActionResult> MakeHost(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                if (!await _userManager.IsInRoleAsync(user, "Host"))
                {
                    var result =
                        await _userManager.AddToRoleAsync(user, "Host");

                    if (result.Succeeded)
                    {
                        TempData["Success"] =
                            "User successfully became Host.";
                    }
                    else
                    {
                        TempData["Error"] =
                            string.Join(", ",
                            result.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    TempData["Error"] =
                        "User is already a Host.";
                }
            }

            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> MakeAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    var result =
                        await _userManager.AddToRoleAsync(user, "Admin");

                    if (result.Succeeded)
                    {
                        TempData["Success"] =
                            "User successfully became Admin.";
                    }
                    else
                    {
                        TempData["Error"] =
                            string.Join(", ",
                            result.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    TempData["Error"] =
                        "User is already an Admin.";
                }
            }

            return RedirectToAction(nameof(Users));
        }


        public async Task<IActionResult> RemoveHost(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, "Host");
            }

            return RedirectToAction(nameof(Users));
        }
        public async Task<IActionResult> ActivityLogs()
        {
            var logs = await _context.ActivityLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(logs);
        }

    }
}