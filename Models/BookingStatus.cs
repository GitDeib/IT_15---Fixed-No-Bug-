namespace IT15_Project.Models
{
    public enum BookingStatus
    {
        Pending,    // The ride has been requested, but not yet confirmed.
        Accepted,   // The driver has accepted the ride.
        Started,    // The ride is in progress.
        Completed,  // The ride has been completed.
        Cancelled   // The ride was cancelled by either the user or the driver.
    }

    public enum PaymentStatus
    {
        Pending,
        Paid, 
        Unpaid   
    }
}
