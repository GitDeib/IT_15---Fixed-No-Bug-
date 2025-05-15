using System.Collections.Generic;

namespace IT15_Project.Models
{
    public class DriverDashboardViewModel
    {
        public Driver Driver { get; set; }
        public FareSetting FareSetting { get; set; }
        public Booking ActiveBooking { get; set; }
        public List<Booking> AvailablePassengers { get; set; }
        public bool IsAvailable { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public Booking AcceptedRide { get; set; }
        public Booking StartedRide { get; set; }
    }
}
