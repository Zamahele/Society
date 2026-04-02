using System.ComponentModel.DataAnnotations;

namespace SocietyApp.ViewModels;

public class CreateClerkViewModel
{
    [Required] [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required] [Display(Name = "ID Number / Username")]
    public string IDNumber { get; set; } = string.Empty;

    [Required] [EmailAddress] [Display(Name = "Email (optional)")]
    public string? Email { get; set; }

    [Required] [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required] [DataType(DataType.Password)] [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
