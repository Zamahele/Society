using SocietyApp.Models;

namespace SocietyApp.Services.Interfaces;

public interface IMembershipService
{
    Task<string> GenerateMembershipNumberAsync();
    Task<Membership?> GetByUserIdAsync(string userId);
    Task<Membership?> GetByIdAsync(int id);
    Task<List<Membership>> GetAllAsync();
    Task<Membership> CreateAsync(string userId);
    Task ActivateAsync(int membershipId);
    Task SuspendAsync(int membershipId);
    Task CheckAndSuspendIfOverdueAsync(int membershipId);
    Task<bool> CanAddDependantAsync(int membershipId);
    Task AddDependantAsync(MemberDependant dependant);
    Task<List<MemberDependant>> GetDependantsAsync(int membershipId);
    Task RemoveDependantAsync(int dependantId);
    Task<MemberNominee?> GetNomineeAsync(int membershipId);
    Task SaveNomineeAsync(int membershipId, string fullName, string idNumber, string phone, string relationship);
    Task RemoveNomineeAsync(int membershipId);
}
