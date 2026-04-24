using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocietyApp.Models;

namespace SocietyApp.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Membership> Memberships { get; set; }
    public DbSet<MemberDependant> MemberDependants { get; set; }
    public DbSet<JoiningFeePayment> JoiningFeePayments { get; set; }
    public DbSet<MonthlyPayment> MonthlyPayments { get; set; }
    public DbSet<DeathClaim> DeathClaims { get; set; }
    public DbSet<PublicSiteSettings> PublicSiteSettings { get; set; }
    public DbSet<CommitteeMember> CommitteeMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("society");

        // Membership
        builder.Entity<Membership>(e =>
        {
            e.HasIndex(m => m.MembershipNumber).IsUnique();
            e.Property(m => m.JoiningFeeAmount).HasColumnType("decimal(18,2)");
            e.Property(m => m.MonthlyFeeAmount).HasColumnType("decimal(18,2)");
            e.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // MemberDependant — max 10 enforced in service layer
        builder.Entity<MemberDependant>(e =>
        {
            e.HasOne(d => d.Membership)
                .WithMany(m => m.Dependants)
                .HasForeignKey(d => d.MembershipId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JoiningFeePayment
        builder.Entity<JoiningFeePayment>(e =>
        {
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            e.HasOne(p => p.Membership)
                .WithMany(m => m.JoiningFeePayments)
                .HasForeignKey(p => p.MembershipId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.SubmittedByClerk)
                .WithMany()
                .HasForeignKey(p => p.SubmittedByClerkId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(p => p.ConfirmedByClerk)
                .WithMany()
                .HasForeignKey(p => p.ConfirmedByClerkId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // MonthlyPayment
        builder.Entity<MonthlyPayment>(e =>
        {
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            e.HasOne(p => p.Membership)
                .WithMany(m => m.MonthlyPayments)
                .HasForeignKey(p => p.MembershipId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.SubmittedByClerk)
                .WithMany()
                .HasForeignKey(p => p.SubmittedByClerkId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(p => p.ConfirmedByClerk)
                .WithMany()
                .HasForeignKey(p => p.ConfirmedByClerkId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // DeathClaim
        builder.Entity<DeathClaim>(e =>
        {
            e.Property(c => c.CashAmount).HasColumnType("decimal(18,2)");
            e.Property(c => c.VoucherAmount).HasColumnType("decimal(18,2)");
            e.HasOne(c => c.Membership)
                .WithMany(m => m.DeathClaims)
                .HasForeignKey(c => c.MembershipId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Dependant)
                .WithMany()
                .HasForeignKey(c => c.DependantId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(c => c.SubmittedByClerk)
                .WithMany()
                .HasForeignKey(c => c.SubmittedByClerkId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(c => c.ProcessedByAdmin)
                .WithMany()
                .HasForeignKey(c => c.ProcessedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<PublicSiteSettings>(e =>
        {
            e.Property(s => s.OrganizationName).HasMaxLength(200);
            e.Property(s => s.RegistrationNumber).HasMaxLength(100);
            e.Property(s => s.EnterpriseType).HasMaxLength(120);
            e.Property(s => s.EnterpriseStatus).HasMaxLength(120);
        });

        builder.Entity<CommitteeMember>(e =>
        {
            e.Property(c => c.FullName).HasMaxLength(150);
            e.Property(c => c.RoleTitle).HasMaxLength(80);
            e.Property(c => c.Phone).HasMaxLength(40);
        });
    }
}
