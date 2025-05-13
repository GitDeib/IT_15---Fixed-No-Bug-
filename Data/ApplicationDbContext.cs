using IT15_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace IT15_Project.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Driver> Drivers { get; set; }

        public DbSet<FareSetting> FareSettings { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<RatingReview> RatingReviews { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // One-to-one config
            builder.Entity<ApplicationUser>()
                .HasOne(a => a.DriverProfile)
                .WithOne(d => d.ApplicationUser)
                .HasForeignKey<Driver>(d => d.UserId);

            builder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            // Configuring the relationship between Booking and ApplicationUser (Driver)
            builder.Entity<Booking>()
                .HasOne(b => b.Driver)
                .WithMany()
                .HasForeignKey(b => b.DriverId)
                .OnDelete(DeleteBehavior.SetNull);  

            // Configuring the relationship between Booking and FareSetting
            builder.Entity<Booking>()
                .HasOne(b => b.FareSetting)
                .WithMany()
                .HasForeignKey(b => b.FareSettingsId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuring the relationship between Booking and RatingReview
            builder.Entity<Booking>()
                .HasMany(b => b.RatingsReviews)
                .WithOne(rr => rr.Booking)
                .HasForeignKey(rr => rr.BookingId)
                .OnDelete(DeleteBehavior.Cascade);


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


            builder.Entity<FareSetting>(entity =>
            {
                entity.Property(f => f.BaseFare).HasPrecision(10, 2);
                entity.Property(f => f.PerKilometerRate).HasPrecision(10, 2);
                entity.Property(f => f.PerMinuteRate).HasPrecision(10, 2);
               
            });

            builder.Entity<FareSetting>().HasData(
        new FareSetting
        {
            Id = 1,
            VehicleType = "Car",
            SeatType = "4-seater",
            BaseFare = 50.00M,
            PerKilometerRate = 15.00M,
            PerMinuteRate = 2.00M,
            DriverShareRate = 80,
            CommissionRate = 20
        },
         new FareSetting
         {
             Id = 2,
             VehicleType = "Car",
             SeatType = "6-seater",
             BaseFare = 50.00M,
             PerKilometerRate = 18.00M,
             PerMinuteRate = 2.00M,
             DriverShareRate = 80,
             CommissionRate = 20
         },
        new FareSetting
        {
            Id = 3,
            VehicleType = "Motorcycle",
            SeatType = "1-seater",
            BaseFare = 30.00M,
            PerKilometerRate = 5.00M,
            PerMinuteRate = 1.50M,
            DriverShareRate = 90,
            CommissionRate = 10
        }
         );

        }
    }
}
