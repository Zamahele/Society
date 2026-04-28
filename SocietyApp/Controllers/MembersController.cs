using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;
using SocietyApp.ViewModels;

namespace SocietyApp.Controllers;

[Authorize(Roles = "Member,Admin,Clerk")]
public class MembersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMembershipService _membershipService;
    private readonly IPaymentService _paymentService;
    private readonly IClaimService _claimService;

    public MembersController(UserManager<ApplicationUser> userManager,
        IMembershipService membershipService,
        IPaymentService paymentService,
        IClaimService claimService)
    {
        _userManager = userManager;
        _membershipService = membershipService;
        _paymentService = paymentService;
        _claimService = claimService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var membership = await _membershipService.GetByUserIdAsync(user.Id);
        if (membership == null) return RedirectToAction("Dashboard", "Admin");

        await _membershipService.CheckAndSuspendIfOverdueAsync(membership.Id);

        var payments = await _paymentService.GetMonthlyHistoryAsync(membership.Id);
        var claims = await _claimService.GetByMembershipAsync(membership.Id);
        var dependants = await _membershipService.GetDependantsAsync(membership.Id);
        var canAdd = await _membershipService.CanAddDependantAsync(membership.Id);

        ViewBag.MonthlyPayments = payments;
        ViewBag.Claims = claims;
        ViewBag.Dependants = dependants;
        ViewBag.CanAdd = canAdd;
        return View(membership);
    }

    public async Task<IActionResult> Dependants()
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = await _membershipService.GetByUserIdAsync(user!.Id);
        if (membership == null) return RedirectToAction("Dashboard", "Admin");

        var dependants = await _membershipService.GetDependantsAsync(membership.Id);
        ViewBag.MembershipId = membership.Id;
        ViewBag.CanAdd = await _membershipService.CanAddDependantAsync(membership.Id);
        return View(dependants);
    }

    [HttpGet]
    public async Task<IActionResult> AddDependant()
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = await _membershipService.GetByUserIdAsync(user!.Id);
        if (membership == null) return RedirectToAction("Dashboard", "Admin");

        var canAdd = await _membershipService.CanAddDependantAsync(membership.Id);
        if (!canAdd)
        {
            TempData["Error"] = "You have reached the maximum of 10 dependants.";
            return RedirectToAction(nameof(Dependants));
        }

        return View(new AddDependantViewModel { MembershipId = membership.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDependant(AddDependantViewModel model, string? returnTo = null)
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = user == null ? null : await _membershipService.GetByUserIdAsync(user.Id);
        if (membership == null) return RedirectToAction("Dashboard", "Admin");

        // Always bind dependant additions to the currently logged-in member.
        model.MembershipId = membership.Id;

        string redirectTarget = returnTo == "Dashboard" ? nameof(Dashboard) : nameof(Dependants);

        if (!ModelState.IsValid) return View(model);

        var canAdd = await _membershipService.CanAddDependantAsync(membership.Id);
        if (!canAdd)
        {
            TempData["Error"] = "You have reached the maximum of 10 dependants.";
            return RedirectToAction(redirectTarget);
        }

        var dependant = new MemberDependant
        {
            MembershipId = membership.Id,
            FullName = model.FullName,
            IDNumber = model.IDNumber,
            DateOfBirth = model.DateOfBirth,
            Relationship = model.Relationship
        };

        await _membershipService.AddDependantAsync(dependant);
        TempData["Success"] = "Dependant added successfully.";
        return RedirectToAction(redirectTarget);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveDependant(int id, string? returnTo = null)
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = user == null ? null : await _membershipService.GetByUserIdAsync(user.Id);
        if (membership == null) return RedirectToAction("Dashboard", "Admin");

        var ownDependants = await _membershipService.GetDependantsAsync(membership.Id);
        string redirectTarget = returnTo == "Dashboard" ? nameof(Dashboard) : nameof(Dependants);

        if (!ownDependants.Any(d => d.Id == id))
        {
            TempData["Error"] = "Dependant not found for your membership.";
            return RedirectToAction(redirectTarget);
        }

        await _membershipService.RemoveDependantAsync(id);
        TempData["Success"] = "Dependant removed.";
        return RedirectToAction(redirectTarget);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveNominee(string fullName, string idNumber, string phone, string relationship)
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = user == null ? null : await _membershipService.GetByUserIdAsync(user.Id);
        if (membership == null) return RedirectToAction("Dashboard", "Admin");

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(idNumber))
        {
            TempData["Error"] = "Full name and ID number are required.";
            return RedirectToAction(nameof(Dashboard));
        }

        await _membershipService.SaveNomineeAsync(membership.Id, fullName.Trim(), idNumber.Trim(), phone?.Trim() ?? string.Empty, relationship?.Trim() ?? string.Empty);
        TempData["Success"] = "Nominee saved successfully.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveNominee()
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = user == null ? null : await _membershipService.GetByUserIdAsync(user.Id);
        if (membership == null) return RedirectToAction("Dashboard", "Admin");

        await _membershipService.RemoveNomineeAsync(membership.Id);
        TempData["Success"] = "Nominee removed.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        return View(new EditProfileViewModel
        {
            FullName = user.FullName,
            Phone = user.Phone,
            Address = user.Address
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (!ModelState.IsValid) return View(model);

        user.FullName = model.FullName.Trim();
        user.Phone = model.Phone.Trim();
        user.Address = model.Address.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Dashboard));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditDependant(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = user == null ? null : await _membershipService.GetByUserIdAsync(user.Id);
        if (membership == null) return RedirectToAction("Dashboard", "Admin");

        var dependants = await _membershipService.GetDependantsAsync(membership.Id);
        var dependant = dependants.FirstOrDefault(d => d.Id == id);
        if (dependant == null) return NotFound();

        return View(new EditDependantViewModel
        {
            Id = dependant.Id,
            MembershipId = membership.Id,
            FullName = dependant.FullName,
            IDNumber = dependant.IDNumber,
            DateOfBirth = dependant.DateOfBirth,
            Relationship = dependant.Relationship
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDependant(EditDependantViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = user == null ? null : await _membershipService.GetByUserIdAsync(user.Id);
        if (membership == null) return RedirectToAction("Dashboard", "Admin");

        var dependants = await _membershipService.GetDependantsAsync(membership.Id);
        if (!dependants.Any(d => d.Id == model.Id))
        {
            TempData["Error"] = "Dependant not found for your membership.";
            return RedirectToAction(nameof(Dashboard));
        }

        if (!ModelState.IsValid) return View(model);

        await _membershipService.UpdateDependantAsync(model.Id, model.FullName.Trim(), model.IDNumber.Trim(), model.DateOfBirth, model.Relationship);
        TempData["Success"] = "Dependant updated successfully.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> BankingDetails()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var model = new UpdateBankingDetailsViewModel
        {
            BankAccountName = user.BankAccountName,
            BankAccountNumber = user.BankAccountNumber,
            BankName = user.BankName
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BankingDetails(UpdateBankingDetailsViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        user.BankAccountName = model.BankAccountName?.Trim() ?? string.Empty;
        user.BankAccountNumber = model.BankAccountNumber?.Trim() ?? string.Empty;
        user.BankName = model.BankName?.Trim() ?? string.Empty;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "Banking details updated successfully.";
            return RedirectToAction(nameof(Dashboard));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }
}
