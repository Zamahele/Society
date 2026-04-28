using System.ComponentModel.DataAnnotations;

namespace SocietyApp.ViewModels;

public class UpdateBankingDetailsViewModel
{
    [Display(Name = "Bank Account Name")]
    public string BankAccountName { get; set; } = string.Empty;

    [Display(Name = "Bank Account Number")]
    public string BankAccountNumber { get; set; } = string.Empty;

    [Display(Name = "Bank Name")]
    public string BankName { get; set; } = string.Empty;
}
