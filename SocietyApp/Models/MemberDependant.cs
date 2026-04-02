namespace SocietyApp.Models;

public class MemberDependant
{
    public int Id { get; set; }
    public int MembershipId { get; set; }
    public Membership Membership { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public string IDNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DependantRelationship Relationship { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}

public enum DependantRelationship
{
    Spouse,
    Child,
    Parent,
    Sibling,
    Other
}
