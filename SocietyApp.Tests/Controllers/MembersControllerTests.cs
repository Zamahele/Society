using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SocietyApp.Controllers;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;
using SocietyApp.Tests.TestSupport;

namespace SocietyApp.Tests.Controllers;

public class MembersControllerTests
{
    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private sealed class StubMembershipService : IMembershipService
    {
        public Membership? MembershipToReturn { get; set; }
        public bool CanAdd { get; set; } = true;

        public Task<Membership?> GetByUserIdAsync(string userId) => Task.FromResult(MembershipToReturn);
        public Task<Membership?> GetByIdAsync(int id) => Task.FromResult(MembershipToReturn);
        public Task<List<Membership>> GetAllAsync() => Task.FromResult(new List<Membership>());
        public Task<string> GenerateMembershipNumberAsync() => Task.FromResult("SOC-0001");
        public Task<Membership> CreateAsync(string userId) => throw new NotImplementedException();
        public Task ActivateAsync(int membershipId) => Task.CompletedTask;
        public Task SuspendAsync(int membershipId) => Task.CompletedTask;
        public Task CancelAsync(int membershipId) => Task.CompletedTask;
        public Task CheckAndSuspendIfOverdueAsync(int membershipId) => Task.CompletedTask;
        public Task<bool> CanAddDependantAsync(int membershipId) => Task.FromResult(CanAdd);
        public Task AddDependantAsync(MemberDependant dependant) => Task.CompletedTask;
        public Task<List<MemberDependant>> GetDependantsAsync(int membershipId) => Task.FromResult(new List<MemberDependant>());
        public Task RemoveDependantAsync(int dependantId) => Task.CompletedTask;
        public Task UpdateDependantAsync(int dependantId, string fullName, string idNumber, DateTime dateOfBirth, DependantRelationship relationship) => Task.CompletedTask;
        public Task<MemberNominee?> GetNomineeAsync(int membershipId) => Task.FromResult<MemberNominee?>(null);
        public Task SaveNomineeAsync(int membershipId, string fullName, string idNumber, string phone, string relationship) => Task.CompletedTask;
        public Task RemoveNomineeAsync(int membershipId) => Task.CompletedTask;
    }

    private sealed class StubClaimService : IClaimService
    {
        public Task<List<DeathClaim>> GetAllAsync() => Task.FromResult(new List<DeathClaim>());
        public Task<ClaimEligibilityResult> CheckEligibilityAsync(int membershipId) => Task.FromResult(new ClaimEligibilityResult { IsEligible = false });
        public Task<DeathClaim?> GetByIdAsync(int claimId) => Task.FromResult<DeathClaim?>(null);
        public Task<List<DeathClaim>> GetByMembershipAsync(int membershipId) => Task.FromResult(new List<DeathClaim>());
        public Task<DeathClaim> SubmitClaimAsync(int membershipId, DeathClaim claim, byte[]? certificate, string? fileName, string? submittedByClerkId = null) => throw new NotImplementedException();
        public Task MoveToUnderReviewAsync(int claimId) => Task.CompletedTask;
        public Task ApproveAsync(int claimId, string adminId) => Task.CompletedTask;
        public Task RejectAsync(int claimId, string adminId, string reason) => Task.CompletedTask;
        public Task RecordCashPayoutAsync(int claimId, string adminId) => Task.CompletedTask;
        public Task RecordVoucherPayoutAsync(int claimId, string adminId, string voucherReference) => Task.CompletedTask;
    }

    private static MembersController BuildController(
        ApplicationUser? user = null,
        StubMembershipService? membership = null,
        IPaymentService? payments = null,
        IClaimService? claims = null)
    {
        var userManager = new FakeUserManager(user ?? new ApplicationUser { Id = "user1", FullName = "Test User" });
        var controller = new MembersController(
            userManager,
            membership ?? new StubMembershipService(),
            payments ?? new StubPaymentService(),
            claims ?? new StubClaimService());

        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
        return controller;
    }

    [Fact]
    public async Task Dashboard_WhenMembershipNotFound_RedirectsToAdminDashboard()
    {
        var membershipSvc = new StubMembershipService { MembershipToReturn = null };
        var controller = BuildController(membership: membershipSvc);

        var result = await controller.Dashboard() as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Dashboard", result!.ActionName);
        Assert.Equal("Admin", result.ControllerName);
    }

    [Fact]
    public async Task Dashboard_WithActiveMembership_ReturnsViewWithModel()
    {
        var membership = new Membership
        {
            Id = 1,
            MembershipNumber = "SOC-0001",
            Status = MembershipStatus.Active,
            DateActivated = DateTime.UtcNow.AddMonths(-3),
            User = new ApplicationUser { FullName = "Test User" },
            Dependants = new List<MemberDependant>()
        };
        var membershipSvc = new StubMembershipService { MembershipToReturn = membership };
        var controller = BuildController(membership: membershipSvc);

        var result = await controller.Dashboard() as ViewResult;

        Assert.NotNull(result);
        Assert.Equal(membership, result!.Model);
        Assert.NotNull(controller.ViewBag.MonthlyPayments);
        Assert.NotNull(controller.ViewBag.Claims);
        Assert.NotNull(controller.ViewBag.Dependants);
    }

    [Fact]
    public async Task Dashboard_ActiveMemberWithDateActivated_SetsWaitingMonthsElapsed()
    {
        var activatedDate = DateTime.UtcNow.AddMonths(-2);
        var membership = new Membership
        {
            Id = 1,
            Status = MembershipStatus.Active,
            DateActivated = activatedDate,
            User = new ApplicationUser { FullName = "Test User" },
            Dependants = new List<MemberDependant>()
        };
        var membershipSvc = new StubMembershipService { MembershipToReturn = membership };
        var controller = BuildController(membership: membershipSvc);

        await controller.Dashboard();

        var elapsed = controller.ViewBag.WaitingMonthsElapsed as int?;
        Assert.NotNull(elapsed);
        Assert.True(elapsed >= 0 && elapsed <= 6);
    }

    [Fact]
    public async Task Dashboard_WaitingMonthsCappedAtSix_WhenMemberActivatedLongAgo()
    {
        var membership = new Membership
        {
            Id = 1,
            Status = MembershipStatus.Active,
            DateActivated = DateTime.UtcNow.AddMonths(-24),
            User = new ApplicationUser { FullName = "Test User" },
            Dependants = new List<MemberDependant>()
        };
        var membershipSvc = new StubMembershipService { MembershipToReturn = membership };
        var controller = BuildController(membership: membershipSvc);

        await controller.Dashboard();

        Assert.Equal(6, controller.ViewBag.WaitingMonthsElapsed);
    }
}
