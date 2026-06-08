using System.ComponentModel.DataAnnotations;

namespace Staybnb.Models
{
    public class HostApplication
    {
        public int Id { get; set; }

        [Required]
        public string PropertyTitle { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal PricePerNight { get; set; }

        [Required]
        public string Address { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime SubmittedAt { get; set; } = DateTime.Now;


        // RELATIONSHIP TO USER
        public string? UserId { get; set; }

        public ApplicationUser User { get; set; }
    }
}