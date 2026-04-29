using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SocietyApp.ViewModels;

public class SubmitPaymentViewModel
{
    public int MembershipId { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;

    [Required]
    public string PaymentType { get; set; } = string.Empty; // "JoiningFee" or "Monthly"

    public DateTime? ForMonth { get; set; }

    [Required]
    public string PaymentReference { get; set; } = string.Empty;

    public IFormFile? Proof { get; set; }
}
