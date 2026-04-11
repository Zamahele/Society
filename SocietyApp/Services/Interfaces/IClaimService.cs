using SocietyApp.Models;

namespace SocietyApp.Services.Interfaces;

public interface IClaimService
{
    Task<ClaimEligibilityResult> CheckEligibilityAsync(int membershipId);
    Task<DeathClaim> SubmitClaimAsync(int membershipId, DeathClaim claim, byte[]? certificate, string? fileName, string? submittedByClerkId = null);
    Task<DeathClaim?> GetByIdAsync(int claimId);
    Task<List<DeathClaim>> GetByMembershipAsync(int membershipId);
    Task<List<DeathClaim>> GetAllAsync();
    Task MoveToUnderReviewAsync(int claimId);
    Task ApproveAsync(int claimId, string adminId);
    Task RejectAsync(int claimId, string adminId, string reason);
    Task RecordCashPayoutAsync(int claimId, string adminId);
    Task RecordVoucherPayoutAsync(int claimId, string adminId, string voucherReference);
}

public class ClaimEligibilityResult
{
    public bool IsEligible { get; set; }
    public List<string> Reasons { get; set; } = new();
}
