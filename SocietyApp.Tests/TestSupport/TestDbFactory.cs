using Microsoft.EntityFrameworkCore;
using SocietyApp.Data;

namespace SocietyApp.Tests.TestSupport;

internal static class TestDbFactory
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AppDbContext(options);
    }
}
