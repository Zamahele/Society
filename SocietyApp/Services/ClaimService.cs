using Microsoft.EntityFrameworkCore;
using SocietyApp.Data;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;

namespace SocietyApp.Services;

public class ClaimService : IClaimService
{
    private readonly AppDbContext _db;
    private readonly IPaymentService _paymentService;
    private const int WaitingPeriodMonths = 6;

    public ClaimService(AppDbContext db, IPaymentService paymentService)
    {
        _db = db;
        _paymentService = paymentService;
    }

    public async Task<ClaimEligibilityResult> CheckEligibilityAsync(int membershipId)
    {
        var result = new ClaimEligibilityResult { IsEligible = true };
        var membership = await _db.Memberships.FindAsync(membershipId);

        if (membership == null)
        {
            result.IsEligible = false;
            result.Reasons.Add("Membership not found.");
            return result;
        }

        if (membership.Status != MembershipStatus.Active)
        {
            result.IsEligible = false;
            result.Reasons.Add($"Membership is not active (current status: {membership.Status}).");
        }

        if (membership.DateActivated == null ||
            membership.DateActivated.Value.AddMonths(WaitingPeriodMonths) > DateTime.UtcNow)
        {
            result.IsEligible = false;
            var activatedOn = membership.DateActivated?.ToString("dd MMM yyyy") ?? "not yet";
            result.Reasons.Add($"6-month waiting period not met. Activated on {activatedOn}.");
        }

        var isOverdue = await _paymentService.IsOverdueAsync(membershipId);
        if (isOverdue)
        {
            result.IsEligible = false;
            result.Reasons.Add("Monthly payment is overdue by more than 30 days.");
        }

        return result;
    }

    public async Task<DeathClaim> SubmitClaimAsync(int membershipId, DeathClaim claim, byte[]? certificate, string? fileName, string? submittedByClerkId = null)
    {
        claim.MembershipId = membershipId;
        claim.ClaimDate = DateTime.UtcNow;
        claim.ClaimStatus = ClaimStatus.Submitted;
        claim.CashAmount = 15000m;
        claim.VoucherAmount = 15000m;
        claim.SubmittedByClerkId = submittedByClerkId;

        if (certificate != null)
        {
            claim.DeathCertificateData = certificate;
            claim.DeathCertificateFileName = fileName;
        }

        _db.DeathClaims.Add(claim);
        await _db.SaveChangesAsync();
        return claim;
    }

    public async Task<DeathClaim?> GetByIdAsync(int claimId)
    {
        return await _db.DeathClaims
            .Include(c => c.Membership)
                .ThenInclude(m => m.User)
            .Include(c => c.Dependant)
            .Include(c => c.ProcessedByAdmin)
            .FirstOrDefaultAsync(c => c.Id == claimId);
    }

    public async Task<List<DeathClaim>> GetByMembershipAsync(int membershipId)
    {
        return await _db.DeathClaims
            .Where(c => c.MembershipId == membershipId)
            .OrderByDescending(c => c.ClaimDate)
            .ToListAsync();
    }

    public async Task<List<DeathClaim>> GetAllAsync()
    {
        return await _db.DeathClaims
            .Include(c => c.Membership)
                .ThenInclude(m => m.User)
            .Include(c => c.Dependant)
            .OrderByDescending(c => c.ClaimDate)
            .ToListAsync();
    }

    public async Task MoveToUnderReviewAsync(int claimId)
    {
        var claim = await _db.DeathClaims.FindAsync(claimId);
        if (claim == null) return;

        claim.ClaimStatus = ClaimStatus.UnderReview;
        await _db.SaveChangesAsync();
    }

    public async Task ApproveAsync(int claimId, string adminId)
    {
        var claim = await _db.DeathClaims.FindAsync(claimId);
        if (claim == null) return;

        claim.ClaimStatus = ClaimStatus.Approved;
        claim.ProcessedByAdminId = adminId;
        await _db.SaveChangesAsync();
    }

    public async Task RejectAsync(int claimId, string adminId, string reason)
    {
        var claim = await _db.DeathClaims.FindAsync(claimId);
        if (claim == null) return;

        claim.ClaimStatus = ClaimStatus.Rejected;
        claim.ProcessedByAdminId = adminId;
        claim.RejectionReason = reason;
        await _db.SaveChangesAsync();
    }

    public async Task RecordCashPayoutAsync(int claimId, string adminId)
    {
        var claim = await _db.DeathClaims.FindAsync(claimId);
        if (claim == null) return;

        claim.CashPaidDate = DateTime.UtcNow;
        claim.ProcessedByAdminId = adminId;
        claim.ClaimStatus = claim.VoucherPaidDate.HasValue
            ? ClaimStatus.FullyPaid
            : ClaimStatus.PartiallyPaid;

        await _db.SaveChangesAsync();
    }

    public async Task RecordVoucherPayoutAsync(int claimId, string adminId, string voucherReference)
    {
        var claim = await _db.DeathClaims.FindAsync(claimId);
        if (claim == null) return;

        claim.VoucherPaidDate = DateTime.UtcNow;
        claim.VoucherReference = voucherReference;
        claim.ProcessedByAdminId = adminId;
        claim.ClaimStatus = claim.CashPaidDate.HasValue
            ? ClaimStatus.FullyPaid
            : ClaimStatus.PartiallyPaid;

        await _db.SaveChangesAsync();
    }
}
