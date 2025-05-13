using System.ComponentModel.DataAnnotations;

namespace IT15_Project.Models
{
    public class FareSetting
    {
        public int Id { get; set; }
        public string VehicleType { get; set; }
        public string SeatType { get; set; }
        public decimal BaseFare { get; set; }
        public decimal PerKilometerRate { get; set; }
        public decimal PerMinuteRate { get; set; }
        public int DriverShareRate { get; set; }
        public int CommissionRate { get; set; }
    }
}
