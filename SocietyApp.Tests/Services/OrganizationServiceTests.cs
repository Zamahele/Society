using SocietyApp.Models;
using SocietyApp.Services;
using SocietyApp.Tests.TestSupport;

namespace SocietyApp.Tests.Services;

public class OrganizationServiceTests
{
    [Fact]
    public async Task AddMemberAsync_SetsOrganizationAndPendingStatus()
    {
        using var db = TestDbFactory.CreateContext();
        var organization = new Organization
        {
            UserId = "organization-user-1",
            Name = "Example Organization",
            RegistrationNumber = "ORG-001"
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var service = new OrganizationService(db);
        var member = await service.AddMemberAsync(organization.Id, new OrganizationMember
        {
            FullName = "Person One",
            IDNumber = "8001011234088",
            Phone = "0712345678",
            DateOfBirth = new DateTime(1980, 1, 1)
        });

        Assert.Equal(organization.Id, member.OrganizationId);
        Assert.Equal(OrganizationMemberStatus.Pending, member.Status);
        Assert.NotEqual(default, member.DateAdded);
    }

    [Fact]
    public async Task AddMemberAsync_RejectsDuplicateActiveMemberForOrganization()
    {
        using var db = TestDbFactory.CreateContext();
        var organization = new Organization { UserId = "organization-user-2", Name = "Example", RegistrationNumber = "ORG-002" };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var service = new OrganizationService(db);
        await service.AddMemberAsync(organization.Id, new OrganizationMember
        {
            FullName = "Person One",
            IDNumber = "8001011234088",
            Phone = "0712345678",
            DateOfBirth = new DateTime(1980, 1, 1)
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddMemberAsync(organization.Id, new OrganizationMember
            {
                FullName = "Person One Duplicate",
                IDNumber = "8001011234088",
                Phone = "0798765432",
                DateOfBirth = new DateTime(1980, 1, 1)
            }));

        Assert.Contains("already registered", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RemoveMemberAsync_DoesNotRemoveAnotherOrganizationsMember()
    {
        using var db = TestDbFactory.CreateContext();
        var firstOrganization = new Organization { UserId = "organization-user-3", Name = "First", RegistrationNumber = "ORG-003" };
        var secondOrganization = new Organization { UserId = "organization-user-4", Name = "Second", RegistrationNumber = "ORG-004" };
        db.Organizations.AddRange(firstOrganization, secondOrganization);
        await db.SaveChangesAsync();

        var member = new OrganizationMember
        {
            OrganizationId = secondOrganization.Id,
            FullName = "Second Organization Person",
            IDNumber = "8101011234088",
            Phone = "0712345678",
            DateOfBirth = new DateTime(1981, 1, 1)
        };
        db.OrganizationMembers.Add(member);
        await db.SaveChangesAsync();

        var removed = await new OrganizationService(db).RemoveMemberAsync(firstOrganization.Id, member.Id);

        Assert.False(removed);
        Assert.Equal(OrganizationMemberStatus.Pending, (await db.OrganizationMembers.FindAsync(member.Id))!.Status);
    }

    [Fact]
    public async Task RemoveMemberAsync_SoftRemovesMemberAndExcludesItFromList()
    {
        using var db = TestDbFactory.CreateContext();
        var organization = new Organization { UserId = "organization-user-5", Name = "Example", RegistrationNumber = "ORG-005" };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var member = new OrganizationMember
        {
            OrganizationId = organization.Id,
            FullName = "Person To Remove",
            IDNumber = "8201011234088",
            Phone = "0712345678",
            DateOfBirth = new DateTime(1982, 1, 1)
        };
        db.OrganizationMembers.Add(member);
        await db.SaveChangesAsync();

        var service = new OrganizationService(db);
        Assert.True(await service.RemoveMemberAsync(organization.Id, member.Id));

        Assert.Equal(OrganizationMemberStatus.Removed, (await db.OrganizationMembers.FindAsync(member.Id))!.Status);
        Assert.Empty(await service.GetMembersAsync(organization.Id));
    }
}