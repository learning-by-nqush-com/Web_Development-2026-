using System.ComponentModel.DataAnnotations;

namespace Staybnb.Models
{
    public class Property
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public decimal PricePerNight { get; set; }

        public int MaxGuests { get; set; }

        public double AverageRating
        {
            get
            {
                if (Reviews == null || !Reviews.Any())
                {
                    return 0;
                }

                return Reviews.Average(r => r.Rating);
            }
        }

        [Required]
        public string Address { get; set; }

        public bool IsActive { get; set; }

        public string HostId { get; set; }

        public ApplicationUser Host { get; set; }

        public List<PropertyImage> Images { get; set; }
            = new List<PropertyImage>();

        public List<Booking> Bookings { get; set; }
           = new List<Booking>();

        public List<Review> Reviews { get; set; }
            = new List<Review>();


    }
}