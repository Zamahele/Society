using SocietyApp.Models;

namespace SocietyApp.ViewModels;

public class PublicLandingViewModel
{
    public PublicSiteSettings Settings { get; set; } = new();
    public List<CommitteeMember> CommitteeMembers { get; set; } = new();
}

public class PublicContentAdminViewModel
{
    public PublicSiteSettings Settings { get; set; } = new();
    public List<CommitteeMember> CommitteeMembers { get; set; } = new();

    public string NewMemberName { get; set; } = string.Empty;
    public string NewMemberRole { get; set; } = string.Empty;
    public string NewMemberPhone { get; set; } = string.Empty;
}
