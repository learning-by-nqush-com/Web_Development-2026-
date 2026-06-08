using Microsoft.AspNetCore.Identity;

namespace Staybnb.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}