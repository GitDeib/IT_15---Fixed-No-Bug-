using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

public class RideBookingModel : PageModel
{
    [BindProperty]
    public RideBookingInputModel Input { get; set; }

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // TODO: Save booking to DB, redirect to success page, etc.
        return RedirectToPage("/RideConfirmation");
    }
}

public class RideBookingInputModel
{
    [Required]
    public string PickupLocation { get; set; }

    [Required]
    public string DropOffLocation { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime RideDate { get; set; }

    [Required]
    [DataType(DataType.Time)]
    public TimeSpan RideTime { get; set; }

    [Required]
    public string RideType { get; set; }

    public string Notes { get; set; }
}
