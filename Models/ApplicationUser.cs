using Microsoft.AspNetCore.Identity;

namespace IT15_Project.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = "";

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = "";

        public string Address { get; set; } = "";

        public string? ProfilePhotoPath { get; set; } 

        public DateTime CreatedAt { get; set; }

        
        // Navigation: One-to-One with Driver
        public virtual Driver DriverProfile { get; set; }

    }
}
