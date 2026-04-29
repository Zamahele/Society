using SocietyApp.Models;
using SocietyApp.Services.Interfaces;

namespace SocietyApp.Tests.TestSupport;

internal sealed class StubPaymentService : IPaymentService
{
    public bool IsOverdueResult { get; set; }

    public Task<bool> IsOverdueAsync(int membershipId) => Task.FromResult(IsOverdueResult);

    public Task<JoiningFeePayment> SubmitJoiningFeeAsync(int membershipId, string reference, DateTime paymentDate, string? submittedByClerkId = null, byte[]? proofData = null, string? proofFileName = null)
        => throw new NotImplementedException();

    public Task ConfirmJoiningFeeAsync(int paymentId, string clerkId)
        => throw new NotImplementedException();

    public Task<List<JoiningFeePayment>> GetPendingJoiningFeesAsync()
        => throw new NotImplementedException();

    public Task<JoiningFeePayment?> GetJoiningFeeByIdAsync(int id)
        => throw new NotImplementedException();

    public Task<bool> HasPendingJoiningFeeAsync(int membershipId) => Task.FromResult(false);
    public Task<List<JoiningFeePayment>> GetJoiningFeesByMembershipAsync(int membershipId) => Task.FromResult(new List<JoiningFeePayment>());

    public Task<MonthlyPayment> SubmitMonthlyPaymentAsync(int membershipId, DateTime forMonth, string reference, DateTime paymentDate, string? submittedByClerkId = null, byte[]? proofData = null, string? proofFileName = null)
        => throw new NotImplementedException();

    public Task ConfirmMonthlyPaymentAsync(int paymentId, string clerkId)
        => throw new NotImplementedException();

    public Task<List<MonthlyPayment>> GetPendingMonthlyPaymentsAsync()
        => throw new NotImplementedException();

    public Task<List<MonthlyPayment>> GetMonthlyHistoryAsync(int membershipId)
        => Task.FromResult(new List<MonthlyPayment>());

    public Task<MonthlyPayment?> GetMonthlyPaymentByIdAsync(int id)
        => throw new NotImplementedException();

    public Task DeleteJoiningFeeAsync(int paymentId) => Task.CompletedTask;
    public Task DeleteMonthlyPaymentAsync(int paymentId) => Task.CompletedTask;
}
