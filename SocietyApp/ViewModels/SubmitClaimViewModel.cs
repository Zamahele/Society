using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SocietyApp.Models;

namespace SocietyApp.ViewModels;

public class SubmitClaimViewModel
{
    public int MembershipId { get; set; }

    [Required] [Display(Name = "Who Passed Away")]
    public DeceasedType DeceasedType { get; set; }

    [Display(Name = "Dependant (if applicable)")]
    public int? DependantId { get; set; }

    [Required] [Display(Name = "Full Name of Deceased")]
    public string DeceasedFullName { get; set; } = string.Empty;

    [Required] [DataType(DataType.Date)] [Display(Name = "Date of Death")]
    public DateTime DateOfDeath { get; set; }

    [Required] [Display(Name = "Death Certificate")]
    public IFormFile DeathCertificate { get; set; } = null!;

    public List<MemberDependant> Dependants { get; set; } = new();
}
