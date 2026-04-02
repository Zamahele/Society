using SocietyApp.Models;

namespace SocietyApp.Services.Interfaces;

public interface IPaymentService
{
    // Joining fee
    Task<JoiningFeePayment> SubmitJoiningFeeAsync(int membershipId, string reference, DateTime paymentDate);
    Task ConfirmJoiningFeeAsync(int paymentId, string clerkId);
    Task<List<JoiningFeePayment>> GetPendingJoiningFeesAsync();
    Task<JoiningFeePayment?> GetJoiningFeeByIdAsync(int id);

    // Monthly payments
    Task<MonthlyPayment> SubmitMonthlyPaymentAsync(int membershipId, DateTime forMonth, string reference, DateTime paymentDate);
    Task ConfirmMonthlyPaymentAsync(int paymentId, string clerkId);
    Task<List<MonthlyPayment>> GetPendingMonthlyPaymentsAsync();
    Task<List<MonthlyPayment>> GetMonthlyHistoryAsync(int membershipId);
    Task<MonthlyPayment?> GetMonthlyPaymentByIdAsync(int id);

    // Overdue check — returns true if any payment is overdue beyond 30 days
    Task<bool> IsOverdueAsync(int membershipId);
}
