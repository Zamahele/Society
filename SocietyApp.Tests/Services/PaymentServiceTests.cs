using Microsoft.EntityFrameworkCore;
using SocietyApp.Models;
using SocietyApp.Services;
using SocietyApp.Tests.TestSupport;

namespace SocietyApp.Tests.Services;

public class PaymentServiceTests
{
    [Fact]
    public async Task ConfirmJoiningFeeAsync_ConfirmsPaymentAndActivatesPendingMembership()
    {
        using var db = TestDbFactory.CreateContext();
        var service = new PaymentService(db);

        var membership = new Membership
        {
            MembershipNumber = "SOC-9001",
            UserId = "member-1",
            Status = MembershipStatus.Pending,
            DateIssued = DateTime.UtcNow
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync();

        var payment = await service.SubmitJoiningFeeAsync(membership.Id, "SOC-9001", DateTime.UtcNow.AddDays(-1));
        await service.ConfirmJoiningFeeAsync(payment.Id, "clerk-1");

        var updatedPayment = await db.JoiningFeePayments.FindAsync(payment.Id);
        var updatedMembership = await db.Memberships.FindAsync(membership.Id);

        Assert.NotNull(updatedPayment);
        Assert.NotNull(updatedMembership);
        Assert.Equal(PaymentStatus.Confirmed, updatedPayment!.Status);
        Assert.Equal("clerk-1", updatedPayment.ConfirmedByClerkId);
        Assert.Equal(MembershipStatus.Active, updatedMembership!.Status);
        Assert.NotNull(updatedMembership.DateActivated);
    }

    [Fact]
    public async Task ConfirmJoiningFeeAsync_ActivatesPendingPaymentMembership()
    {
        using var db = TestDbFactory.CreateContext();
        var service = new PaymentService(db);

        var membership = new Membership
        {
            MembershipNumber = "SOC-9002",
            UserId = "member-2",
            Status = MembershipStatus.PendingPayment,
            DateIssued = DateTime.UtcNow
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync();

        var payment = await service.SubmitJoiningFeeAsync(membership.Id, "SOC-9002", DateTime.UtcNow.AddDays(-1));
        await service.ConfirmJoiningFeeAsync(payment.Id, "clerk-2");

        var updatedMembership = await db.Memberships.FindAsync(membership.Id);

        Assert.NotNull(updatedMembership);
        Assert.Equal(MembershipStatus.Active, updatedMembership!.Status);
        Assert.NotNull(updatedMembership.DateActivated);
    }

    [Fact]
    public async Task SubmitMonthlyPaymentAsync_NormalizesForMonthToFirstDay()
    {
        using var db = TestDbFactory.CreateContext();
        var service = new PaymentService(db);

        db.Memberships.Add(new Membership
        {
            MembershipNumber = "SOC-1000",
            UserId = "member-month",
            Status = MembershipStatus.Active,
            DateIssued = DateTime.UtcNow,
            DateActivated = DateTime.UtcNow.AddMonths(-2)
        });
        await db.SaveChangesAsync();

        var membership = await db.Memberships.FirstAsync();
        var payment = await service.SubmitMonthlyPaymentAsync(
            membership.Id,
            new DateTime(2026, 4, 18),
            "APR-2026",
            DateTime.UtcNow);

        Assert.Equal(new DateTime(2026, 4, 1), payment.ForMonth);
        Assert.Equal(MonthlyPaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public async Task IsOverdueAsync_ReturnsTrueWhenOldExpectedMonthIsUnpaid()
    {
        using var db = TestDbFactory.CreateContext();
        var service = new PaymentService(db);

        var membership = new Membership
        {
            MembershipNumber = "SOC-2000",
            UserId = "member-overdue",
            Status = MembershipStatus.Active,
            DateIssued = DateTime.UtcNow.AddMonths(-4),
            DateActivated = DateTime.UtcNow.AddMonths(-3)
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync();

        var overdue = await service.IsOverdueAsync(membership.Id);

        Assert.True(overdue);
    }

    [Fact]
    public async Task IsOverdueAsync_ReturnsFalseWithinGraceWindow()
    {
        using var db = TestDbFactory.CreateContext();
        var service = new PaymentService(db);

        var membership = new Membership
        {
            MembershipNumber = "SOC-3000",
            UserId = "member-grace",
            Status = MembershipStatus.Active,
            DateIssued = DateTime.UtcNow.AddDays(-25),
            DateActivated = DateTime.UtcNow.AddDays(-20)
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync();

        var overdue = await service.IsOverdueAsync(membership.Id);

        Assert.False(overdue);
    }
}
