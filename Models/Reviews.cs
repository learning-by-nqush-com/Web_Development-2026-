using System.ComponentModel.DataAnnotations;

namespace Staybnb.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public int PropertyId { get; set; }

        public Property Property { get; set; }

        public string GuestId { get; set; }

        public ApplicationUser Guest { get; set; }
    }
}