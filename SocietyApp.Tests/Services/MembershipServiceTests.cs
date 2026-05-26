using Microsoft.EntityFrameworkCore;
using SocietyApp.Models;
using SocietyApp.Services;
using SocietyApp.Tests.TestSupport;

namespace SocietyApp.Tests.Services;

public class MembershipServiceTests
{
    [Fact]
    public async Task CreateAsync_GeneratesSequentialMembershipNumbers()
    {
        using var db = TestDbFactory.CreateContext();
        var payments = new StubPaymentService();
        var service = new MembershipService(db, payments);

        var first = await service.CreateAsync("user-1");
        var second = await service.CreateAsync("user-2");

        Assert.Equal("SOC-0001", first.MembershipNumber);
        Assert.Equal("SOC-0002", second.MembershipNumber);
        Assert.Equal(MembershipStatus.Pending, second.Status);
    }

    [Fact]
    public async Task AddDependantAsync_ThrowsWhenLimitReached()
    {
        using var db = TestDbFactory.CreateContext();
        var payments = new StubPaymentService();
        var service = new MembershipService(db, payments);

        var membership = await service.CreateAsync("member-limit");

        for (var i = 0; i < 10; i++)
        {
            db.MemberDependants.Add(new MemberDependant
            {
                MembershipId = membership.Id,
                FullName = $"Dependant {i + 1}",
                IDNumber = $"90010100000{i}",
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
                Relationship = DependantRelationship.Child
            });
        }

        await db.SaveChangesAsync();

        var canAdd = await service.CanAddDependantAsync(membership.Id);
        Assert.False(canAdd);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddDependantAsync(new MemberDependant
            {
                MembershipId = membership.Id,
                FullName = "Overflow Dependant",
                IDNumber = "8001015555088",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                Relationship = DependantRelationship.Other
            }));
    }

    [Fact]
    public async Task ApproveMembershipAsync_ActivatesPendingMemberAndConfirmsJoiningFee()
    {
        using var db = TestDbFactory.CreateContext();
        var payments = new StubPaymentService();
        var service = new MembershipService(db, payments);

        var membership = await service.CreateAsync("member-approve");
        db.JoiningFeePayments.Add(new JoiningFeePayment
        {
            MembershipId = membership.Id,
            PaymentReference = membership.MembershipNumber,
            PaymentDate = DateTime.UtcNow.AddDays(-1),
            Status = PaymentStatus.Pending
        });
        await db.SaveChangesAsync();

        var ok = await service.ApproveMembershipAsync(membership.Id, "admin-1");

        Assert.True(ok);
        var updated = await db.Memberships.FindAsync(membership.Id);
        var fee = await db.JoiningFeePayments.FirstAsync(p => p.MembershipId == membership.Id);
        Assert.Equal(MembershipStatus.Active, updated!.Status);
        Assert.NotNull(updated.DateActivated);
        Assert.Equal(PaymentStatus.Confirmed, fee.Status);
        Assert.Equal("admin-1", fee.ConfirmedByClerkId);
        Assert.NotNull(fee.ConfirmedDate);
    }

    [Fact]
    public async Task ApproveMembershipAsync_ReturnsFalseWhenNoJoiningFeeRecord()
    {
        using var db = TestDbFactory.CreateContext();
        var payments = new StubPaymentService();
        var service = new MembershipService(db, payments);

        var membership = await service.CreateAsync("member-no-fee");

        var ok = await service.ApproveMembershipAsync(membership.Id, "admin-1");

        Assert.False(ok);
        var updated = await db.Memberships.FindAsync(membership.Id);
        Assert.Equal(MembershipStatus.Pending, updated!.Status);
        Assert.Null(updated.DateActivated);
    }

    [Fact]
    public async Task ApproveMembershipAsync_ReturnsFalseForAlreadyActiveMember()
    {
        using var db = TestDbFactory.CreateContext();
        var payments = new StubPaymentService();
        var service = new MembershipService(db, payments);

        var membership = await service.CreateAsync("member-already-active");
        membership.Status = MembershipStatus.Active;
        membership.DateActivated = DateTime.UtcNow.AddDays(-7);
        db.JoiningFeePayments.Add(new JoiningFeePayment
        {
            MembershipId = membership.Id,
            PaymentReference = membership.MembershipNumber,
            PaymentDate = DateTime.UtcNow.AddDays(-1),
            Status = PaymentStatus.Pending
        });
        await db.SaveChangesAsync();

        var ok = await service.ApproveMembershipAsync(membership.Id, "admin-1");

        Assert.False(ok);
    }

    [Fact]
    public async Task CheckAndSuspendIfOverdueAsync_SuspendsActiveMembership()
    {
        using var db = TestDbFactory.CreateContext();
        var payments = new StubPaymentService { IsOverdueResult = true };
        var service = new MembershipService(db, payments);

        var membership = await service.CreateAsync("member-overdue");
        membership.Status = MembershipStatus.Active;
        membership.DateActivated = DateTime.UtcNow.AddMonths(-3);
        await db.SaveChangesAsync();

        await service.CheckAndSuspendIfOverdueAsync(membership.Id);

        var updated = await db.Memberships.FindAsync(membership.Id);
        Assert.NotNull(updated);
        Assert.Equal(MembershipStatus.Suspended, updated!.Status);
    }
}
