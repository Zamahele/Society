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

    [Display(Name = "Bank Account Name")]
    public string BankAccountName { get; set; } = string.Empty;

    [Display(Name = "Bank Account Number")]
    public string BankAccountNumber { get; set; } = string.Empty;

    [Display(Name = "Bank Name")]
    public string BankName { get; set; } = string.Empty;

    [Required] [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required] [DataType(DataType.Password)] [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
