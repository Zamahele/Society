using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using SocietyApp.Models;
using System.Security.Claims;

namespace SocietyApp.Tests.TestSupport;

internal sealed class FakeUserManager : UserManager<ApplicationUser>
{
    private readonly ApplicationUser? _user;

    public FakeUserManager(ApplicationUser? user = null)
        : base(
            new StubUserStore(),
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<ApplicationUser>>.Instance)
    {
        _user = user;
    }

    public override Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal)
        => Task.FromResult(_user);

    private sealed class StubUserStore : IUserStore<ApplicationUser>
    {
        public void Dispose() { }
        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken ct) => Task.FromResult<ApplicationUser?>(null);
        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct) => Task.FromResult<ApplicationUser?>(null);
        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult<string?>(null);
        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.Id);
        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult<string?>(user.UserName);
        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken ct) => Task.CompletedTask;
        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken ct) => Task.CompletedTask;
        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
    }
}
