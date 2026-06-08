using System.ComponentModel.DataAnnotations;

namespace Staybnb.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public int Guests { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; }

        // Foreign Keys
        public int PropertyId { get; set; }

        public string GuestId { get; set; }

        // Navigation Properties
        public Property Property { get; set; }

        public ApplicationUser Guest { get; set; }
    }
}