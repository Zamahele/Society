using System.ComponentModel.DataAnnotations;

namespace SocietyApp.ViewModels;

public class SubmitJoiningFeeViewModel
{
    public int MembershipId { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;

    [Required] [Display(Name = "EFT Reference / Proof of Payment Reference")]
    public string PaymentReference { get; set; } = string.Empty;

    [Required] [DataType(DataType.Date)] [Display(Name = "Date of Payment")]
    public DateTime PaymentDate { get; set; } = DateTime.Today;
}
