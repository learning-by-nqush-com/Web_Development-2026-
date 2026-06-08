using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;

namespace Staybnb.Models

{
    public class PropertyImage
    {
        public int Id { get; set; }

        public string? ImageUrl { get; set; }

        public int HostApplicationId { get; set; }

        public HostApplication HostApplication { get; set; }
        public int PropertyId { get; set; }

        public Property Property { get; set; }

        private readonly IWebHostEnvironment _environment;

        
    }

}