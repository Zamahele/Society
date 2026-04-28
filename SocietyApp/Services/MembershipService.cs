using Microsoft.EntityFrameworkCore;
using SocietyApp.Data;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;

namespace SocietyApp.Services;

public class MembershipService : IMembershipService
{
    private readonly AppDbContext _db;
    private readonly IPaymentService _paymentService;

    public MembershipService(AppDbContext db, IPaymentService paymentService)
    {
        _db = db;
        _paymentService = paymentService;
    }

    public async Task<string> GenerateMembershipNumberAsync()
    {
        var last = await _db.Memberships
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync();

        int next = 1;
        if (last != null)
        {
            var numberPart = last.MembershipNumber.Replace("SOC-", "");
            if (int.TryParse(numberPart, out int parsed))
                next = parsed + 1;
        }

        return $"SOC-{next:D4}";
    }

    public async Task<Membership> CreateAsync(string userId)
    {
        var membershipNumber = await GenerateMembershipNumberAsync();

        var membership = new Membership
        {
            MembershipNumber = membershipNumber,
            UserId = userId,
            Status = MembershipStatus.Pending,
            DateIssued = DateTime.UtcNow
        };

        _db.Memberships.Add(membership);
        await _db.SaveChangesAsync();
        return membership;
    }

    public async Task<Membership?> GetByUserIdAsync(string userId)
    {
        return await _db.Memberships
            .Include(m => m.Dependants)
            .Include(m => m.JoiningFeePayments)
            .Include(m => m.MonthlyPayments)
            .FirstOrDefaultAsync(m => m.UserId == userId);
    }

    public async Task<Membership?> GetByIdAsync(int id)
    {
        return await _db.Memberships
            .Include(m => m.User)
            .Include(m => m.Dependants)
            .Include(m => m.JoiningFeePayments)
            .Include(m => m.MonthlyPayments)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<Membership>> GetAllAsync()
    {
        return await _db.Memberships
            .Include(m => m.User)
            .Include(m => m.Dependants)
            .OrderByDescending(m => m.DateIssued)
            .ToListAsync();
    }

    public async Task ActivateAsync(int membershipId)
    {
        var membership = await _db.Memberships.FindAsync(membershipId);
        if (membership == null) return;

        membership.Status = MembershipStatus.PendingPayment;
        await _db.SaveChangesAsync();
    }

    public async Task SuspendAsync(int membershipId)
    {
        var membership = await _db.Memberships.FindAsync(membershipId);
        if (membership == null) return;

        membership.Status = MembershipStatus.Suspended;
        await _db.SaveChangesAsync();
    }

    public async Task CheckAndSuspendIfOverdueAsync(int membershipId)
    {
        var membership = await _db.Memberships.FindAsync(membershipId);
        if (membership == null || membership.Status != MembershipStatus.Active) return;

        var isOverdue = await _paymentService.IsOverdueAsync(membershipId);
        if (isOverdue)
            await SuspendAsync(membershipId);
    }

    public async Task<bool> CanAddDependantAsync(int membershipId)
    {
        var count = await _db.MemberDependants
            .CountAsync(d => d.MembershipId == membershipId);
        return count < 10;
    }

    public async Task AddDependantAsync(MemberDependant dependant)
    {
        var canAdd = await CanAddDependantAsync(dependant.MembershipId);
        if (!canAdd)
            throw new InvalidOperationException("Maximum of 10 dependants allowed per membership.");

        _db.MemberDependants.Add(dependant);
        await _db.SaveChangesAsync();
    }

    public async Task<List<MemberDependant>> GetDependantsAsync(int membershipId)
    {
        return await _db.MemberDependants
            .Where(d => d.MembershipId == membershipId)
            .OrderBy(d => d.DateAdded)
            .ToListAsync();
    }

    public async Task RemoveDependantAsync(int dependantId)
    {
        var dependant = await _db.MemberDependants.FindAsync(dependantId);
        if (dependant == null) return;

        _db.MemberDependants.Remove(dependant);
        await _db.SaveChangesAsync();
    }
}
