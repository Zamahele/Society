namespace SocietyApp.Models;

public class CommitteeMember
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
