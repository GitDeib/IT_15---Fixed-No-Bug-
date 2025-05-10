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

        public DbSet<Driver> Drivers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // One-to-one config
            builder.Entity<ApplicationUser>()
                .HasOne(a => a.DriverProfile)
                .WithOne(d => d.ApplicationUser)
                .HasForeignKey<Driver>(d => d.UserId);


            //Roles
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
