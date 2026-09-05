using Microsoft.EntityFrameworkCore;
using SocietyApp.Data;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;

namespace SocietyApp.Services;

public class OrganizationService : IOrganizationService
{
    private readonly AppDbContext _db;

    public OrganizationService(AppDbContext db) => _db = db;

    public Task<Organization?> GetByUserIdAsync(string userId) =>
        _db.Organizations.FirstOrDefaultAsync(o => o.UserId == userId);

    public Task<List<OrganizationMember>> GetMembersAsync(int organizationId) =>
        _db.OrganizationMembers
            .Where(m => m.OrganizationId == organizationId && m.Status != OrganizationMemberStatus.Removed)
            .OrderBy(m => m.FullName)
            .ToListAsync();

    public async Task<OrganizationMember> AddMemberAsync(int organizationId, OrganizationMember member)
    {
        var duplicate = await _db.OrganizationMembers.AnyAsync(m =>
            m.OrganizationId == organizationId && m.IDNumber == member.IDNumber && m.Status != OrganizationMemberStatus.Removed);
        if (duplicate)
            throw new InvalidOperationException("This person is already registered under your organization.");

        member.OrganizationId = organizationId;
        member.Status = OrganizationMemberStatus.Pending;
        member.DateAdded = DateTime.UtcNow;
        _db.OrganizationMembers.Add(member);
        await _db.SaveChangesAsync();
        return member;
    }

    public async Task<bool> RemoveMemberAsync(int organizationId, int memberId)
    {
        var member = await _db.OrganizationMembers.FirstOrDefaultAsync(m =>
            m.Id == memberId && m.OrganizationId == organizationId && m.Status != OrganizationMemberStatus.Removed);
        if (member == null) return false;

        member.Status = OrganizationMemberStatus.Removed;
        await _db.SaveChangesAsync();
        return true;
    }
}