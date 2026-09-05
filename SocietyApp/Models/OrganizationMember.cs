namespace SocietyApp.Models;

public class OrganizationMember
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public string IDNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    public OrganizationMemberStatus Status { get; set; } = OrganizationMemberStatus.Pending;
}

public enum OrganizationMemberStatus
{
    Pending = 0,
    Active = 1,
    Removed = 2
}