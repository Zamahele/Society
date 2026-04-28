using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;
using SocietyApp.ViewModels;

namespace SocietyApp.Controllers;

[Authorize(Roles = "Member")]
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
        if (membership == null) return NotFound();

        await _membershipService.CheckAndSuspendIfOverdueAsync(membership.Id);

        var payments = await _paymentService.GetMonthlyHistoryAsync(membership.Id);
        var claims = await _claimService.GetByMembershipAsync(membership.Id);

        ViewBag.MonthlyPayments = payments;
        ViewBag.Claims = claims;
        return View(membership);
    }

    public async Task<IActionResult> Dependants()
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = await _membershipService.GetByUserIdAsync(user!.Id);
        if (membership == null) return NotFound();

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
        if (membership == null) return NotFound();

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
    public async Task<IActionResult> AddDependant(AddDependantViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = user == null ? null : await _membershipService.GetByUserIdAsync(user.Id);
        if (membership == null) return NotFound();

        // Always bind dependant additions to the currently logged-in member.
        model.MembershipId = membership.Id;

        if (!ModelState.IsValid) return View(model);

        var canAdd = await _membershipService.CanAddDependantAsync(membership.Id);
        if (!canAdd)
        {
            TempData["Error"] = "You have reached the maximum of 10 dependants.";
            return RedirectToAction(nameof(Dependants));
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
        return RedirectToAction(nameof(Dependants));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveDependant(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = user == null ? null : await _membershipService.GetByUserIdAsync(user.Id);
        if (membership == null) return NotFound();

        var ownDependants = await _membershipService.GetDependantsAsync(membership.Id);
        if (!ownDependants.Any(d => d.Id == id))
        {
            TempData["Error"] = "Dependant not found for your membership.";
            return RedirectToAction(nameof(Dependants));
        }

        await _membershipService.RemoveDependantAsync(id);
        TempData["Success"] = "Dependant removed.";
        return RedirectToAction(nameof(Dependants));
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
