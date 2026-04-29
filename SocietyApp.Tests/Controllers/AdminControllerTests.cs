using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SocietyApp.Controllers;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;
using SocietyApp.Tests.TestSupport;

namespace SocietyApp.Tests.Controllers;

public class AdminControllerTests
{
    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }

    // ── Stubs ──────────────────────────────────────────────────────────

    private sealed class StubMembershipService : IMembershipService
    {
        public List<Membership> Members { get; set; } = new();

        public Task<List<Membership>> GetAllAsync() => Task.FromResult(Members);
        public Task<Membership?> GetByUserIdAsync(string userId) => Task.FromResult<Membership?>(null);
        public Task<Membership?> GetByIdAsync(int id) => Task.FromResult(Members.Find(m => m.Id == id));
        public Task<string> GenerateMembershipNumberAsync() => Task.FromResult("SOC-0001");
        public Task<Membership> CreateAsync(string userId) => throw new NotImplementedException();
        public Task ActivateAsync(int membershipId) { var m = Members.Find(x => x.Id == membershipId); if (m != null) m.Status = MembershipStatus.PendingPayment; return Task.CompletedTask; }
        public Task SuspendAsync(int membershipId) => Task.CompletedTask;
        public Task CheckAndSuspendIfOverdueAsync(int membershipId) => Task.CompletedTask;
        public Task<bool> CanAddDependantAsync(int membershipId) => Task.FromResult(true);
        public Task AddDependantAsync(MemberDependant dependant) => Task.CompletedTask;
        public Task<List<MemberDependant>> GetDependantsAsync(int membershipId) => Task.FromResult(new List<MemberDependant>());
        public Task RemoveDependantAsync(int dependantId) => Task.CompletedTask;
        public Task UpdateDependantAsync(int dependantId, string fullName, string idNumber, DateTime dateOfBirth, SocietyApp.Models.DependantRelationship relationship) => Task.CompletedTask;
        public Task<MemberNominee?> GetNomineeAsync(int membershipId) => Task.FromResult<MemberNominee?>(null);
        public Task SaveNomineeAsync(int membershipId, string fullName, string idNumber, string phone, string relationship) => Task.CompletedTask;
        public Task RemoveNomineeAsync(int membershipId) => Task.CompletedTask;
        public Task CancelAsync(int membershipId) => Task.CompletedTask;
    }

    private sealed class StubClaimService : IClaimService
    {
        public List<DeathClaim> Claims { get; set; } = new();

        public Task<List<DeathClaim>> GetAllAsync() => Task.FromResult(Claims);
        public Task<ClaimEligibilityResult> CheckEligibilityAsync(int membershipId) => Task.FromResult(new ClaimEligibilityResult { IsEligible = true });
        public Task<DeathClaim?> GetByIdAsync(int claimId) => Task.FromResult(Claims.Find(c => c.Id == claimId));
        public Task<List<DeathClaim>> GetByMembershipAsync(int membershipId) => Task.FromResult(new List<DeathClaim>());
        public Task<DeathClaim> SubmitClaimAsync(int membershipId, DeathClaim claim, byte[]? certificate, string? fileName, string? submittedByClerkId = null) => throw new NotImplementedException();
        public Task MoveToUnderReviewAsync(int claimId) => Task.CompletedTask;
        public Task ApproveAsync(int claimId, string adminId) => Task.CompletedTask;
        public Task RejectAsync(int claimId, string adminId, string reason) => Task.CompletedTask;
        public Task RecordCashPayoutAsync(int claimId, string adminId) => Task.CompletedTask;
        public Task RecordVoucherPayoutAsync(int claimId, string adminId, string voucherReference) => Task.CompletedTask;
    }

    private sealed class StubPaymentServiceFull : IPaymentService
    {
        public List<JoiningFeePayment> PendingJoining { get; set; } = new();
        public List<MonthlyPayment> PendingMonthly { get; set; } = new();

        public Task<bool> IsOverdueAsync(int membershipId) => Task.FromResult(false);
        public Task<List<JoiningFeePayment>> GetPendingJoiningFeesAsync() => Task.FromResult(PendingJoining);
        public Task<List<MonthlyPayment>> GetPendingMonthlyPaymentsAsync() => Task.FromResult(PendingMonthly);
        public Task<JoiningFeePayment> SubmitJoiningFeeAsync(int membershipId, string reference, DateTime paymentDate, string? submittedByClerkId = null) => throw new NotImplementedException();
        public Task ConfirmJoiningFeeAsync(int paymentId, string clerkId) => Task.CompletedTask;
        public Task<JoiningFeePayment?> GetJoiningFeeByIdAsync(int id) => Task.FromResult<JoiningFeePayment?>(null);
        public Task<bool> HasPendingJoiningFeeAsync(int membershipId) => Task.FromResult(false);
        public Task<List<JoiningFeePayment>> GetJoiningFeesByMembershipAsync(int membershipId) => Task.FromResult(new List<JoiningFeePayment>());
        public Task<MonthlyPayment> SubmitMonthlyPaymentAsync(int membershipId, DateTime forMonth, string reference, DateTime paymentDate, string? submittedByClerkId = null) => throw new NotImplementedException();
        public Task ConfirmMonthlyPaymentAsync(int paymentId, string clerkId) => Task.CompletedTask;
        public Task<List<MonthlyPayment>> GetMonthlyHistoryAsync(int membershipId) => Task.FromResult(new List<MonthlyPayment>());
        public Task<MonthlyPayment?> GetMonthlyPaymentByIdAsync(int id) => Task.FromResult<MonthlyPayment?>(null);
        public Task DeleteJoiningFeeAsync(int paymentId) => Task.CompletedTask;
        public Task DeleteMonthlyPaymentAsync(int paymentId) => Task.CompletedTask;
    }

    private static AdminController BuildController(
        StubMembershipService? membership = null,
        StubClaimService? claims = null,
        StubPaymentServiceFull? payments = null)
    {
        var db = TestDbFactory.CreateContext();
        var controller = new AdminController(
            userManager: null!,
            dbContext: db,
            membershipService: membership ?? new StubMembershipService(),
            paymentService: payments ?? new StubPaymentServiceFull(),
            claimService: claims ?? new StubClaimService());

        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
        return controller;
    }

    // ── Tests ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Dashboard_ReturnsView_WithCorrectCounts()
    {
        var membershipSvc = new StubMembershipService
        {
            Members = new List<Membership>
            {
                new() { Id = 1, Status = MembershipStatus.Active },
                new() { Id = 2, Status = MembershipStatus.Pending },
                new() { Id = 3, Status = MembershipStatus.PendingPayment },
                new() { Id = 4, Status = MembershipStatus.Suspended },
            }
        };
        var claimSvc = new StubClaimService
        {
            Claims = new List<DeathClaim>
            {
                new() { Id = 1, ClaimStatus = ClaimStatus.Submitted },
                new() { Id = 2, ClaimStatus = ClaimStatus.FullyPaid },
            }
        };

        var controller = BuildController(membership: membershipSvc, claims: claimSvc);
        var result = await controller.Dashboard() as ViewResult;

        Assert.NotNull(result);
        Assert.Equal(4, result!.ViewData["TotalMembers"] is null ? controller.ViewBag.TotalMembers : controller.ViewBag.TotalMembers);
        Assert.Equal(4, controller.ViewBag.TotalMembers);
        Assert.Equal(1, controller.ViewBag.ActiveMembers);
        Assert.Equal(1, controller.ViewBag.PendingMembers);
        Assert.Equal(1, controller.ViewBag.PendingPaymentMembers);
        Assert.Equal(1, controller.ViewBag.SuspendedMembers);
        Assert.Equal(2, controller.ViewBag.TotalClaims);
        Assert.Equal(1, controller.ViewBag.PendingClaims);
    }

    [Fact]
    public async Task Members_ReturnsView_WithMembershipList()
    {
        var membershipSvc = new StubMembershipService
        {
            Members = new List<Membership>
            {
                new() { Id = 1, Status = MembershipStatus.Active },
                new() { Id = 2, Status = MembershipStatus.Pending },
            }
        };

        var controller = BuildController(membership: membershipSvc);
        var result = await controller.Members() as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsAssignableFrom<List<Membership>>(result!.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task ApproveMember_CallsActivate_AndRedirects()
    {
        var membershipSvc = new StubMembershipService
        {
            Members = new List<Membership>
            {
                new() { Id = 7, Status = MembershipStatus.Pending }
            }
        };

        var controller = BuildController(membership: membershipSvc);
        var result = await controller.ApproveMember(7) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("MemberDetails", result!.ActionName);
        Assert.Equal(MembershipStatus.PendingPayment, membershipSvc.Members[0].Status);
    }
}
