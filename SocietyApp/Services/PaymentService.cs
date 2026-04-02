using Microsoft.EntityFrameworkCore;
using SocietyApp.Data;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;

namespace SocietyApp.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private const int GracePeriodDays = 30;

    public PaymentService(AppDbContext db)
    {
        _db = db;
    }

    // --------------- Joining Fee ---------------

    public async Task<JoiningFeePayment> SubmitJoiningFeeAsync(int membershipId, string reference, DateTime paymentDate)
    {
        var payment = new JoiningFeePayment
        {
            MembershipId = membershipId,
            Amount = 150m,
            PaymentReference = reference,
            PaymentDate = paymentDate,
            Status = PaymentStatus.Pending
        };

        _db.JoiningFeePayments.Add(payment);
        await _db.SaveChangesAsync();
        return payment;
    }

    public async Task ConfirmJoiningFeeAsync(int paymentId, string clerkId)
    {
        var payment = await _db.JoiningFeePayments
            .Include(p => p.Membership)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) return;

        payment.Status = PaymentStatus.Confirmed;
        payment.ConfirmedByClerkId = clerkId;
        payment.ConfirmedDate = DateTime.UtcNow;

        // Activate membership on joining fee confirmation
        if (payment.Membership.Status == MembershipStatus.Pending)
        {
            payment.Membership.Status = MembershipStatus.Active;
            payment.Membership.DateActivated = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<JoiningFeePayment>> GetPendingJoiningFeesAsync()
    {
        return await _db.JoiningFeePayments
            .Include(p => p.Membership)
                .ThenInclude(m => m.User)
            .Where(p => p.Status == PaymentStatus.Pending)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<JoiningFeePayment?> GetJoiningFeeByIdAsync(int id)
    {
        return await _db.JoiningFeePayments
            .Include(p => p.Membership)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // --------------- Monthly Payments ---------------

    public async Task<MonthlyPayment> SubmitMonthlyPaymentAsync(int membershipId, DateTime forMonth, string reference, DateTime paymentDate)
    {
        var normalizedMonth = new DateTime(forMonth.Year, forMonth.Month, 1);

        var payment = new MonthlyPayment
        {
            MembershipId = membershipId,
            ForMonth = normalizedMonth,
            Amount = 150m,
            PaymentReference = reference,
            PaymentDate = paymentDate,
            Status = MonthlyPaymentStatus.Pending
        };

        _db.MonthlyPayments.Add(payment);
        await _db.SaveChangesAsync();
        return payment;
    }

    public async Task ConfirmMonthlyPaymentAsync(int paymentId, string clerkId)
    {
        var payment = await _db.MonthlyPayments.FindAsync(paymentId);
        if (payment == null) return;

        payment.Status = MonthlyPaymentStatus.Confirmed;
        payment.ConfirmedByClerkId = clerkId;
        payment.ConfirmedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<List<MonthlyPayment>> GetPendingMonthlyPaymentsAsync()
    {
        return await _db.MonthlyPayments
            .Include(p => p.Membership)
                .ThenInclude(m => m.User)
            .Where(p => p.Status == MonthlyPaymentStatus.Pending)
            .OrderBy(p => p.ForMonth)
            .ToListAsync();
    }

    public async Task<List<MonthlyPayment>> GetMonthlyHistoryAsync(int membershipId)
    {
        return await _db.MonthlyPayments
            .Where(p => p.MembershipId == membershipId)
            .OrderByDescending(p => p.ForMonth)
            .ToListAsync();
    }

    public async Task<MonthlyPayment?> GetMonthlyPaymentByIdAsync(int id)
    {
        return await _db.MonthlyPayments
            .Include(p => p.Membership)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // --------------- Overdue Check ---------------

    public async Task<bool> IsOverdueAsync(int membershipId)
    {
        var membership = await _db.Memberships.FindAsync(membershipId);
        if (membership?.DateActivated == null) return false;

        var cutoff = DateTime.UtcNow.AddDays(-GracePeriodDays);
        var activeSince = membership.DateActivated.Value;

        // Build a list of months that should have been paid since activation
        var monthsExpected = new List<DateTime>();
        var current = new DateTime(activeSince.Year, activeSince.Month, 1);
        var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        while (current <= thisMonth)
        {
            monthsExpected.Add(current);
            current = current.AddMonths(1);
        }

        var confirmedMonths = await _db.MonthlyPayments
            .Where(p => p.MembershipId == membershipId && p.Status == MonthlyPaymentStatus.Confirmed)
            .Select(p => p.ForMonth)
            .ToListAsync();

        // Find any month that is unpaid and its due date has passed the grace period
        foreach (var month in monthsExpected)
        {
            if (confirmedMonths.Contains(month)) continue;

            // Due date is the last day of that month; overdue after grace period
            var dueDate = month.AddMonths(1).AddDays(-1);
            if (dueDate.AddDays(GracePeriodDays) < DateTime.UtcNow)
                return true;
        }

        return false;
    }
}
