using System.ComponentModel.DataAnnotations;

namespace SocietyApp.ViewModels;

public class OrganizationRegisterViewModel
{
    [Required] [Display(Name = "Organization Name")]
    public string Name { get; set; } = string.Empty;

    [Required] [Display(Name = "Registration Number")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required] [Display(Name = "Contact Person")]
    public string ContactPerson { get; set; } = string.Empty;

    [Required] [Phone] [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required] [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required] [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required] [DataType(DataType.Password)] [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class AddOrganizationMemberViewModel
{
    [Required] [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required] [Display(Name = "ID Number")]
    public string IDNumber { get; set; } = string.Empty;

    [Required] [Phone] [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [Required] [DataType(DataType.Date)] [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }
}