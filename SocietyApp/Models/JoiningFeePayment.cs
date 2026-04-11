namespace SocietyApp.Models;

public class JoiningFeePayment
{
    public int Id { get; set; }
    public int MembershipId { get; set; }
    public Membership Membership { get; set; } = null!;
    public decimal Amount { get; set; } = 150m;
    public DateTime PaymentDate { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? SubmittedByClerkId { get; set; }
    public ApplicationUser? SubmittedByClerk { get; set; }
    public string? ConfirmedByClerkId { get; set; }
    public ApplicationUser? ConfirmedByClerk { get; set; }
    public DateTime? ConfirmedDate { get; set; }
    public string? Notes { get; set; }
}

public enum PaymentStatus
{
    Pending,
    Confirmed
}
