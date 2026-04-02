using Microsoft.AspNetCore.Identity;
using SocietyApp.Models;

namespace SocietyApp.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Admin", "Clerk", "Member" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed default Admin
        const string adminIdNumber = "0000000000000";
        var admin = await userManager.FindByNameAsync(adminIdNumber);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminIdNumber,
                IDNumber = adminIdNumber,
                FullName = "System Admin",
                Phone = "0000000000",
                Address = "Society Head Office",
                DateOfBirth = new DateTime(1980, 1, 1),
                BankAccountName = "Society Admin",
                BankAccountNumber = "000000000",
                BankName = "Society Bank",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@1234");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
