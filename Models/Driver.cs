using IT15_Project.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Driver
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }  // Foreign Key to ApplicationUser

    [ForeignKey("UserId")]
    public virtual ApplicationUser ApplicationUser { get; set; }

    //Additional Fields
    [Required]
    public string VehicleType { get; set; }

    public string? VehicleSeat { get; set; }

    [Required]
    public string VehicleModel { get; set; }

    [Required]
    public string PlateNumber { get; set; }
    
    [Required]
    public string DriverLicense { get; set; }
    
    [Required]
    public string CertificateRegistration { get; set; }
    
    [Required]
    public string OfficialReceipt { get; set; }

    [Required]
    public string NBIClearance { get; set; }
    
    [Required]
    public string Motivation { get; set; }

    public string Status { get; set; }

}
