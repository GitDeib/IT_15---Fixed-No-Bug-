namespace IT15_Project.Models
{
    public class RatingReview
    {
        public int Id { get; set; }

        // Foreign key to link the rating/review to the booking
        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        // Foreign keys for the users involved (rated by and rated to)
        public int RatedByUserId { get; set; } // User who gave the rating/review
        public int RatedToUserId { get; set; } // The user being rated/reviewed (driver or passenger)

        // Ratings & Reviews
        public int Rating { get; set; } // Rating value (e.g., 1-5)
        public string Review { get; set; } // Text review

        // Optional comment (can be left blank)
        public string? Comment { get; set; } // Optional comment field

        // Role of the person being rated
        public bool IsUserReview { get; set; } // True if it's a user rating the driver, false if the driver is rating the user

        // Timestamp for when the review was submitted
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}