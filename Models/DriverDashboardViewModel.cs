namespace IT15_Project.Models
{
    public class DriverDashboardViewModel
    {
        public List<Booking> AvailablePassengers { get; set; }
        public Booking CurrentRide { get; set; }
        public int DriverId { get; set; }  // FK to Driver
        public virtual Driver Driver { get; set; }
        public bool IsAvailable { get; set; }
        public FareSetting FareSetting { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public Booking AcceptedRide { get; set; }
        public Booking StartedRide { get; set; }
    }
}
