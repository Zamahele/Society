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
        if (!ModelState.IsValid) return View(model);

        var dependant = new MemberDependant
        {
            MembershipId = model.MembershipId,
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
        await _membershipService.RemoveDependantAsync(id);
        TempData["Success"] = "Dependant removed.";
        return RedirectToAction(nameof(Dependants));
    }
}
