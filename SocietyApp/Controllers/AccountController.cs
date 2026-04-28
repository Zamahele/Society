using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;
using SocietyApp.ViewModels;

namespace SocietyApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMembershipService _membershipService;

    public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IMembershipService membershipService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _membershipService = membershipService;
    }

    [HttpGet]
    public IActionResult Register() => View();

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
}
