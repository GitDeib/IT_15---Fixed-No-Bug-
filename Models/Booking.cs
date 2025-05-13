using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IT15_Project.Models
{
    public class Booking
    {
        public int Id { get; set; }

        // Foreign key to AspNetUsers (passenger)
        public string? UserId { get; set; }

        [BindNever] // EF will auto-load this if needed; don't bind from form
        public ApplicationUser? User { get; set; }

        // Optional: if driver is assigned later
        public string? DriverId { get; set; }

        [BindNever]
        public ApplicationUser? Driver { get; set; }

        // Pickup & drop-off
        [Required]
        public string PickupLocation { get; set; }

        [Required]
        public string DropoffLocation { get; set; }

        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }
        public double DropoffLatitude { get; set; }
        public double DropoffLongitude { get; set; }

        // Ride info
        public double DistanceInKm { get; set; }
        public double EstimatedTimeInMinutes { get; set; }
        public double? ActualTimeInMinutes { get; set; }

        // FK to FareSettings
        [Required]
        public int FareSettingsId { get; set; }

        [BindNever] // EF will auto-load
        public FareSetting? FareSetting { get; set; }

        public decimal EstimatedFare { get; set; }
        public decimal? FinalFare { get; set; }

        // Vehicle info
        [Required]
        public string VehicleType { get; set; }

        [Required]
        public string VehicleSeat { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string? PaymentId { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        [BindNever]
        public ICollection<RatingReview>? RatingsReviews { get; set; }
    }
}
