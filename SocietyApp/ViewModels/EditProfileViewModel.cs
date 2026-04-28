using System.ComponentModel.DataAnnotations;

namespace SocietyApp.ViewModels;

public class EditProfileViewModel
{
    [Required] [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required] [Phone] [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;
}
