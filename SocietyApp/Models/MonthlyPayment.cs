namespace SocietyApp.Models;

public class MonthlyPayment
{
    public int Id { get; set; }
    public int MembershipId { get; set; }
    public Membership Membership { get; set; } = null!;

    /// <summary>
    /// Month this payment covers, stored as first day of the month e.g. 2026-04-01
    /// </summary>
    public DateTime ForMonth { get; set; }

    public decimal Amount { get; set; } = 150m;
    public DateTime? PaymentDate { get; set; }
    public string? PaymentReference { get; set; }
    public MonthlyPaymentStatus Status { get; set; } = MonthlyPaymentStatus.Pending;
    public string? ConfirmedByClerkId { get; set; }
    public ApplicationUser? ConfirmedByClerk { get; set; }
    public DateTime? ConfirmedDate { get; set; }
}

public enum MonthlyPaymentStatus
{
    Pending,
    Confirmed,
    Missed
}
