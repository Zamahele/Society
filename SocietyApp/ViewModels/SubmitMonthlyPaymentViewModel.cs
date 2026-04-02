using System.ComponentModel.DataAnnotations;

namespace SocietyApp.ViewModels;

public class SubmitMonthlyPaymentViewModel
{
    public int MembershipId { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;

    [Required] [Display(Name = "Payment For Month")]
    [DataType(DataType.Date)]
    public DateTime ForMonth { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [Required] [Display(Name = "EFT Reference / Proof of Payment Reference")]
    public string PaymentReference { get; set; } = string.Empty;

    [Required] [DataType(DataType.Date)] [Display(Name = "Date of Payment")]
    public DateTime PaymentDate { get; set; } = DateTime.Today;
}
