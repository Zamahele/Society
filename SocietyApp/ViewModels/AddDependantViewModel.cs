using System.ComponentModel.DataAnnotations;
using SocietyApp.Models;

namespace SocietyApp.ViewModels;

public class AddDependantViewModel
{
    public int MembershipId { get; set; }

    [Required] [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required] [Display(Name = "ID Number")]
    public string IDNumber { get; set; } = string.Empty;

    [Required] [DataType(DataType.Date)] [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public DependantRelationship Relationship { get; set; }
}
