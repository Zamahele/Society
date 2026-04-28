using SocietyApp.Models;
using SocietyApp.Services;
using SocietyApp.Tests.TestSupport;

namespace SocietyApp.Tests.Services;

public class ClaimServiceTests
{
    [Fact]
    public async Task CheckEligibilityAsync_ReturnsReasonsWhenMembershipNotActiveAndWaitingPeriodNotMet()
    {
        using var db = TestDbFactory.CreateContext();
        var payments = new StubPaymentService { IsOverdueResult = false };
        var service = new ClaimService(db, payments);

        var membership = new Membership
        {
            MembershipNumber = "SOC-4000",
            UserId = "member-pending",
            Status = MembershipStatus.Pending,
            DateIssued = DateTime.UtcNow
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync();

        var result = await service.CheckEligibilityAsync(membership.Id);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Reasons, r => r.Contains("not active", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Reasons, r => r.Contains("6-month waiting period", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CheckEligibilityAsync_ReturnsEligibleForActiveMemberPastWaitingPeriod()
    {
        using var db = TestDbFactory.CreateContext();
        var payments = new StubPaymentService { IsOverdueResult = false };
        var service = new ClaimService(db, payments);

        var membership = new Membership
        {
            MembershipNumber = "SOC-5000",
            UserId = "member-eligible",
            Status = MembershipStatus.Active,
            DateIssued = DateTime.UtcNow.AddMonths(-7),
            DateActivated = DateTime.UtcNow.AddMonths(-7)
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync();

        var result = await service.CheckEligibilityAsync(membership.Id);

        Assert.True(result.IsEligible);
        Assert.Empty(result.Reasons);
    }

    [Fact]
    public async Task SubmitClaimAsync_SetsDefaultAmountsAndStoresCertificate()
    {
        using var db = TestDbFactory.CreateContext();
        var payments = new StubPaymentService();
        var service = new ClaimService(db, payments);

        var membership = new Membership
        {
            MembershipNumber = "SOC-6000",
            UserId = "member-claim",
            Status = MembershipStatus.Active,
            DateIssued = DateTime.UtcNow.AddMonths(-8),
            DateActivated = DateTime.UtcNow.AddMonths(-7)
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync();

        var claim = await service.SubmitClaimAsync(
            membership.Id,
            new DeathClaim
            {
                DeceasedType = DeceasedType.MainMember,
                DeceasedFullName = "John Example",
                DateOfDeath = DateTime.UtcNow.AddDays(-2)
            },
            new byte[] { 1, 2, 3 },
            "death-certificate.pdf");

        Assert.Equal(ClaimStatus.Submitted, claim.ClaimStatus);
        Assert.Equal(15000m, claim.CashAmount);
        Assert.Equal(15000m, claim.VoucherAmount);
        Assert.Equal("death-certificate.pdf", claim.DeathCertificateFileName);
        Assert.NotNull(claim.DeathCertificateData);
        Assert.Equal(3, claim.DeathCertificateData!.Length);
    }

    [Fact]
    public async Task RecordPayouts_TransitionsClaimToFullyPaid()
    {
        using var db = TestDbFactory.CreateContext();
        var payments = new StubPaymentService();
        var service = new ClaimService(db, payments);

        var membership = new Membership
        {
            MembershipNumber = "SOC-7000",
            UserId = "member-paid",
            Status = MembershipStatus.Active,
            DateIssued = DateTime.UtcNow.AddMonths(-10),
            DateActivated = DateTime.UtcNow.AddMonths(-9)
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync();

        var claim = new DeathClaim
        {
            MembershipId = membership.Id,
            DeceasedType = DeceasedType.MainMember,
            DeceasedFullName = "Paid Claim",
            DateOfDeath = DateTime.UtcNow.AddDays(-5),
            ClaimStatus = ClaimStatus.Approved
        };

        db.DeathClaims.Add(claim);
        await db.SaveChangesAsync();

        await service.RecordCashPayoutAsync(claim.Id, "admin-1");
        var afterCash = await db.DeathClaims.FindAsync(claim.Id);
        Assert.NotNull(afterCash);
        Assert.Equal(ClaimStatus.PartiallyPaid, afterCash!.ClaimStatus);

        await service.RecordVoucherPayoutAsync(claim.Id, "admin-1", "VOUCH-123");
        var afterVoucher = await db.DeathClaims.FindAsync(claim.Id);

        Assert.NotNull(afterVoucher);
        Assert.Equal(ClaimStatus.FullyPaid, afterVoucher!.ClaimStatus);
        Assert.Equal("VOUCH-123", afterVoucher.VoucherReference);
        Assert.NotNull(afterVoucher.CashPaidDate);
        Assert.NotNull(afterVoucher.VoucherPaidDate);
    }
}
