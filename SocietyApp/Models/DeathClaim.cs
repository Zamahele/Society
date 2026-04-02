namespace SocietyApp.Models;

public class DeathClaim
{
    public int Id { get; set; }
    public int MembershipId { get; set; }
    public Membership Membership { get; set; } = null!;

    public DeceasedType DeceasedType { get; set; }
    public int? DependantId { get; set; }
    public MemberDependant? Dependant { get; set; }

    public string DeceasedFullName { get; set; } = string.Empty;
    public DateTime DateOfDeath { get; set; }

    public byte[]? DeathCertificateData { get; set; }
    public string? DeathCertificateFileName { get; set; }

    public DateTime ClaimDate { get; set; } = DateTime.UtcNow;
    public ClaimStatus ClaimStatus { get; set; } = ClaimStatus.Submitted;

    public decimal CashAmount { get; set; } = 15000m;
    public DateTime? CashPaidDate { get; set; }

    public decimal VoucherAmount { get; set; } = 15000m;
    public string? VoucherReference { get; set; }
    public DateTime? VoucherPaidDate { get; set; }

    public string? RejectionReason { get; set; }
    public string? ProcessedByAdminId { get; set; }
    public ApplicationUser? ProcessedByAdmin { get; set; }
}

public enum DeceasedType
{
    MainMember,
    Dependant
}

public enum ClaimStatus
{
    Submitted,
    UnderReview,
    Approved,
    Rejected,
    PartiallyPaid,
    FullyPaid
}
