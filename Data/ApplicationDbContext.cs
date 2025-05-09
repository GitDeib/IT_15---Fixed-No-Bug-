using IT15_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IT15_Project.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var admin = new IdentityRole("Admin");
            admin.NormalizedName = "ADMIN";

            var passenger = new IdentityRole("Passenger");
            passenger.NormalizedName = "PASSENGER";

            var driver = new IdentityRole("Driver");
            driver.NormalizedName = "DRIVER";

            var staff = new IdentityRole("Staff");
            staff.NormalizedName = "STAFF";

            builder.Entity<IdentityRole>().HasData(admin, passenger, driver, staff);
        }
    }
}
