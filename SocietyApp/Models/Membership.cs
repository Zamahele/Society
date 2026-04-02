namespace SocietyApp.Models;

public class Membership
{
    public int Id { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public MembershipStatus Status { get; set; } = MembershipStatus.Pending;
    public decimal JoiningFeeAmount { get; set; } = 150m;
    public decimal MonthlyFeeAmount { get; set; } = 150m;
    public DateTime DateIssued { get; set; } = DateTime.UtcNow;
    public DateTime? DateActivated { get; set; }

    public ICollection<MemberDependant> Dependants { get; set; } = new List<MemberDependant>();
    public ICollection<JoiningFeePayment> JoiningFeePayments { get; set; } = new List<JoiningFeePayment>();
    public ICollection<MonthlyPayment> MonthlyPayments { get; set; } = new List<MonthlyPayment>();
    public ICollection<DeathClaim> DeathClaims { get; set; } = new List<DeathClaim>();
}

public enum MembershipStatus
{
    Pending,
    Active,
    Suspended,
    Cancelled
}
