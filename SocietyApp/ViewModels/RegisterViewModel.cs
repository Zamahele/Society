using System.ComponentModel.DataAnnotations;

namespace SocietyApp.ViewModels;

public class RegisterViewModel
{
    [Required] [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required] [Display(Name = "ID Number")]
    public string IDNumber { get; set; } = string.Empty;

    [Required] [Phone] [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required] [DataType(DataType.Date)] [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Required] [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required] [DataType(DataType.Password)] [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // Optional nominee — can be added later from the Dashboard
    [Display(Name = "Nominee Full Name")]
    public string? NomineeFullName { get; set; }

    [Display(Name = "Nominee ID Number")]
    public string? NomineeIDNumber { get; set; }

    [Display(Name = "Nominee Phone")]
    public string? NomineePhone { get; set; }

    [Display(Name = "Relationship")]
    public string? NomineeRelationship { get; set; }
}
