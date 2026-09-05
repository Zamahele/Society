using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Data;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;
using SocietyApp.ViewModels;

namespace SocietyApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMembershipService _membershipService;
    private readonly AppDbContext _dbContext;

    public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IMembershipService membershipService,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _membershipService = membershipService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpGet]
    public IActionResult RegisterOrganization() => View(new OrganizationRegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterOrganization(OrganizationRegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.Name,
            Phone = model.Phone,
            Address = model.Address,
            DateRegistered = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Organization");
        _dbContext.Organizations.Add(new Organization
        {
            UserId = user.Id,
            Name = model.Name.Trim(),
            RegistrationNumber = model.RegistrationNumber.Trim(),
            ContactPerson = model.ContactPerson.Trim(),
            Phone = model.Phone.Trim(),
            Address = model.Address.Trim()
        });
        await _dbContext.SaveChangesAsync();
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Dashboard", "Organization");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.IDNumber,
            IDNumber = model.IDNumber,
            FullName = model.FullName,
            Phone = model.Phone,
            Address = model.Address,
            DateOfBirth = model.DateOfBirth,
            BankAccountName = string.Empty,
            BankAccountNumber = string.Empty,
            BankName = string.Empty
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Member");
        var membership = await _membershipService.CreateAsync(user.Id);

        if (!string.IsNullOrWhiteSpace(model.NomineeFullName) && !string.IsNullOrWhiteSpace(model.NomineeIDNumber))
        {
            await _membershipService.SaveNomineeAsync(
                membership.Id,
                model.NomineeFullName.Trim(),
                model.NomineeIDNumber.Trim(),
                model.NomineePhone?.Trim() ?? string.Empty,
                model.NomineeRelationship?.Trim() ?? string.Empty);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Dashboard", "Members");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.IDNumber, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid ID Number or password.");
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.IDNumber);
        if (user != null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin") || roles.Contains("Clerk"))
                return RedirectToAction("Dashboard", "Admin");
        }

        return LocalRedirect(returnUrl ?? "/Members/Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();

    // ---- Forgot Password (no email/SMS — verified by DOB + Phone) ----

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByNameAsync(model.IDNumber);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "No account found with that ID number.");
            return View(model);
        }

        return RedirectToAction(nameof(SecurityQuestions), new { userId = user.Id });
    }

    [HttpGet]
    public IActionResult SecurityQuestions(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction(nameof(ForgotPassword));

        return View(new SecurityQuestionsViewModel { UserId = userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SecurityQuestions(SecurityQuestionsViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return RedirectToAction(nameof(ForgotPassword));

        var dobMatch = user.DateOfBirth.Date == model.DateOfBirth.Date;
        var phoneMatch = string.Equals(user.Phone?.Trim(), model.Phone.Trim(), StringComparison.OrdinalIgnoreCase);

        if (!dobMatch || !phoneMatch)
        {
            ModelState.AddModelError(string.Empty, "The answers you provided do not match our records.");
            return View(model);
        }

        return RedirectToAction(nameof(ResetPasswordConfirm), new { userId = user.Id });
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirm(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction(nameof(ForgotPassword));

        return View(new ResetPasswordConfirmViewModel { UserId = userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPasswordConfirm(ResetPasswordConfirmViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return RedirectToAction(nameof(ForgotPassword));

        // Generate and use the token in the same request to avoid serialization issues
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        TempData["Success"] = "Password reset successfully. You can now log in.";
        return RedirectToAction(nameof(Login));
    }
}
