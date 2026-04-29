using System.ComponentModel.DataAnnotations;

namespace SocietyApp.ViewModels;

public class SubmitPaymentViewModel
{
    public int MembershipId { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;

    [Required]
    public string PaymentType { get; set; } = string.Empty; // "JoiningFee" or "Monthly"

    public DateTime? ForMonth { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [Required]
    public string PaymentReference { get; set; } = string.Empty;
}
