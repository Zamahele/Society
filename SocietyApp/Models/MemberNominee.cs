namespace SocietyApp.Models;

public class MemberNominee
{
    public int Id { get; set; }
    public int MembershipId { get; set; }
    public Membership Membership { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public string IDNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}
