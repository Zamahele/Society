using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SocietyApp.Controllers;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;
using SocietyApp.Tests.TestSupport;

namespace SocietyApp.Tests.Controllers;

public class PaymentsControllerTests
{
    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private sealed class TrackingPaymentService : IPaymentService
    {
        public List<JoiningFeePayment> PendingJoining { get; set; } = new();
        public List<MonthlyPayment> PendingMonthly { get; set; } = new();
        public int DeletedJoiningFeeId { get; private set; }
        public int DeletedMonthlyPaymentId { get; private set; }

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
        public Task DeleteJoiningFeeAsync(int paymentId) { DeletedJoiningFeeId = paymentId; return Task.CompletedTask; }
        public Task DeleteMonthlyPaymentAsync(int paymentId) { DeletedMonthlyPaymentId = paymentId; return Task.CompletedTask; }
    }

    private sealed class StubMembershipService : IMembershipService
    {
        public Task<Membership?> GetByUserIdAsync(string userId) => Task.FromResult<Membership?>(null);
        public Task<Membership?> GetByIdAsync(int id) => Task.FromResult<Membership?>(null);
        public Task<List<Membership>> GetAllAsync() => Task.FromResult(new List<Membership>());
        public Task<string> GenerateMembershipNumberAsync() => Task.FromResult("SOC-0001");
        public Task<Membership> CreateAsync(string userId) => throw new NotImplementedException();
        public Task ActivateAsync(int membershipId) => Task.CompletedTask;
        public Task SuspendAsync(int membershipId) => Task.CompletedTask;
        public Task CancelAsync(int membershipId) => Task.CompletedTask;
        public Task CheckAndSuspendIfOverdueAsync(int membershipId) => Task.CompletedTask;
        public Task<bool> CanAddDependantAsync(int membershipId) => Task.FromResult(true);
        public Task AddDependantAsync(MemberDependant dependant) => Task.CompletedTask;
        public Task<List<MemberDependant>> GetDependantsAsync(int membershipId) => Task.FromResult(new List<MemberDependant>());
        public Task RemoveDependantAsync(int dependantId) => Task.CompletedTask;
        public Task UpdateDependantAsync(int dependantId, string fullName, string idNumber, DateTime dateOfBirth, DependantRelationship relationship) => Task.CompletedTask;
        public Task<MemberNominee?> GetNomineeAsync(int membershipId) => Task.FromResult<MemberNominee?>(null);
        public Task SaveNomineeAsync(int membershipId, string fullName, string idNumber, string phone, string relationship) => Task.CompletedTask;
        public Task RemoveNomineeAsync(int membershipId) => Task.CompletedTask;
    }

    private static PaymentsController BuildController(TrackingPaymentService? payments = null)
    {
        var userManager = new FakeUserManager(new ApplicationUser { Id = "clerk1", FullName = "Clerk" });
        var controller = new PaymentsController(
            userManager,
            new StubMembershipService(),
            payments ?? new TrackingPaymentService());

        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
        return controller;
    }

    [Fact]
    public async Task PendingJoiningFees_ReturnsView_WithPaymentList()
    {
        var paymentSvc = new TrackingPaymentService
        {
            PendingJoining = new List<JoiningFeePayment>
            {
                new() { Id = 1, MembershipId = 10, Status = PaymentStatus.Pending },
                new() { Id = 2, MembershipId = 11, Status = PaymentStatus.Pending },
            }
        };
        var controller = BuildController(paymentSvc);

        var result = await controller.PendingJoiningFees() as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsAssignableFrom<List<JoiningFeePayment>>(result!.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task PendingMonthly_ReturnsView_WithPaymentList()
    {
        var paymentSvc = new TrackingPaymentService
        {
            PendingMonthly = new List<MonthlyPayment>
            {
                new() { Id = 5, MembershipId = 10, Status = MonthlyPaymentStatus.Pending },
            }
        };
        var controller = BuildController(paymentSvc);

        var result = await controller.PendingMonthly() as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsAssignableFrom<List<MonthlyPayment>>(result!.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task DeleteJoiningFee_CallsDeleteAndRedirectsToPendingList()
    {
        var paymentSvc = new TrackingPaymentService();
        var controller = BuildController(paymentSvc);

        var result = await controller.DeleteJoiningFee(42) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal(nameof(controller.PendingJoiningFees), result!.ActionName);
        Assert.Equal(42, paymentSvc.DeletedJoiningFeeId);
    }

    [Fact]
    public async Task DeleteMonthly_CallsDeleteAndRedirectsToPendingList()
    {
        var paymentSvc = new TrackingPaymentService();
        var controller = BuildController(paymentSvc);

        var result = await controller.DeleteMonthly(99) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal(nameof(controller.PendingMonthly), result!.ActionName);
        Assert.Equal(99, paymentSvc.DeletedMonthlyPaymentId);
    }
}
