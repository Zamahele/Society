using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;
using SocietyApp.ViewModels;

namespace SocietyApp.Controllers;

[Authorize(Roles = "Admin,Clerk")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMembershipService _membershipService;
    private readonly IPaymentService _paymentService;
    private readonly IClaimService _claimService;

    public AdminController(UserManager<ApplicationUser> userManager,
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
        var memberships = await _membershipService.GetAllAsync();
        var claims = await _claimService.GetAllAsync();
        var pendingJoining = await _paymentService.GetPendingJoiningFeesAsync();
        var pendingMonthly = await _paymentService.GetPendingMonthlyPaymentsAsync();

        ViewBag.TotalMembers = memberships.Count;
        ViewBag.ActiveMembers = memberships.Count(m => m.Status == MembershipStatus.Active);
        ViewBag.PendingMembers = memberships.Count(m => m.Status == MembershipStatus.Pending);
        ViewBag.SuspendedMembers = memberships.Count(m => m.Status == MembershipStatus.Suspended);
        ViewBag.TotalClaims = claims.Count;
        ViewBag.PendingClaims = claims.Count(c => c.ClaimStatus == ClaimStatus.Submitted || c.ClaimStatus == ClaimStatus.UnderReview);
        ViewBag.PendingJoiningFees = pendingJoining.Count;
        ViewBag.PendingMonthlyPayments = pendingMonthly.Count;

        return View();
    }

    public async Task<IActionResult> Members()
    {
        var memberships = await _membershipService.GetAllAsync();
        return View(memberships);
    }

    public async Task<IActionResult> MemberDetails(int id)
    {
        var membership = await _membershipService.GetByIdAsync(id);
        if (membership == null) return NotFound();

        var monthlyHistory = await _paymentService.GetMonthlyHistoryAsync(id);
        var claims = await _claimService.GetByMembershipAsync(id);

        ViewBag.MonthlyHistory = monthlyHistory;
        ViewBag.Claims = claims;
        return View(membership);
    }

    // ---- Create Clerk (Admin only) ----

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult CreateClerk() => View(new CreateClerkViewModel());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClerk(CreateClerkViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.IDNumber,
            IDNumber = model.IDNumber,
            FullName = model.FullName,
            Email = model.Email,
            Phone = string.Empty,
            Address = string.Empty,
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

        await _userManager.AddToRoleAsync(user, "Clerk");
        TempData["Success"] = $"Clerk account created for {model.FullName}.";
        return RedirectToAction(nameof(Dashboard));
    }
}
