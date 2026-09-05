using SocietyApp.Models;

namespace SocietyApp.Services.Interfaces;

public interface IOrganizationService
{
    Task<Organization?> GetByUserIdAsync(string userId);
    Task<List<OrganizationMember>> GetMembersAsync(int organizationId);
    Task<OrganizationMember> AddMemberAsync(int organizationId, OrganizationMember member);
    Task<bool> RemoveMemberAsync(int organizationId, int memberId);
}