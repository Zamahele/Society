using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;
using SocietyApp.ViewModels;

namespace SocietyApp.Controllers;

[Authorize]
public class ClaimsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMembershipService _membershipService;
    private readonly IClaimService _claimService;

    public ClaimsController(UserManager<ApplicationUser> userManager,
        IMembershipService membershipService,
        IClaimService claimService)
    {
        _userManager = userManager;
        _membershipService = membershipService;
        _claimService = claimService;
    }

    // ---- Member: Submit Claim ----

    [Authorize(Roles = "Member")]
    [HttpGet]
    public async Task<IActionResult> Submit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (!HasBankingDetails(user))
        {
            TempData["Error"] = "Please add your banking details before submitting a claim so payouts can be processed.";
            return RedirectToAction("BankingDetails", "Members");
        }

        var membership = await _membershipService.GetByUserIdAsync(user!.Id);
        if (membership == null) return NotFound();

        var eligibility = await _claimService.CheckEligibilityAsync(membership.Id);
        if (!eligibility.IsEligible)
        {
            ViewBag.Reasons = eligibility.Reasons;
            return View("NotEligible");
        }

        var dependants = await _membershipService.GetDependantsAsync(membership.Id);
        return View(new SubmitClaimViewModel
        {
            MembershipId = membership.Id,
            Dependants = dependants
        });
    }

    [Authorize(Roles = "Member")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitClaimViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (!HasBankingDetails(user))
        {
            TempData["Error"] = "Please add your banking details before submitting a claim so payouts can be processed.";
            return RedirectToAction("BankingDetails", "Members");
        }

        if (!ModelState.IsValid)
        {
            model.Dependants = await _membershipService.GetDependantsAsync(model.MembershipId);
            return View(model);
        }

        byte[]? certData = null;
        string? certFileName = null;

        if (model.DeathCertificate != null && model.DeathCertificate.Length > 0)
        {
            using var ms = new MemoryStream();
            await model.DeathCertificate.CopyToAsync(ms);
            certData = ms.ToArray();
            certFileName = model.DeathCertificate.FileName;
        }

        var claim = new DeathClaim
        {
            DeceasedType = model.DeceasedType,
            DependantId = model.DependantId,
            DeceasedFullName = model.DeceasedFullName,
            DateOfDeath = model.DateOfDeath
        };

        try
        {
            await _claimService.SubmitClaimAsync(model.MembershipId, claim, certData, certFileName);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Dependants = await _membershipService.GetDependantsAsync(model.MembershipId);
            return View(model);
        }

        TempData["Success"] = "Claim submitted successfully. We will be in touch.";
        return RedirectToAction(nameof(MyClaims));
    }

    [Authorize(Roles = "Member")]
    public async Task<IActionResult> MyClaims()
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = await _membershipService.GetByUserIdAsync(user!.Id);
        if (membership == null) return NotFound();

        var claims = await _claimService.GetByMembershipAsync(membership.Id);
        return View(claims);
    }

    // ---- Clerk/Admin: All Claims ----

    [Authorize(Roles = "Admin,Clerk")]
    public async Task<IActionResult> Index()
    {
        var claims = await _claimService.GetAllAsync();
        return View(claims);
    }

    [Authorize(Roles = "Admin,Clerk")]
    public async Task<IActionResult> Details(int id)
    {
        var claim = await _claimService.GetByIdAsync(id);
        if (claim == null) return NotFound();
        return View(claim);
    }

    [Authorize(Roles = "Admin,Clerk")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveToReview(int id)
    {
        await _claimService.MoveToUnderReviewAsync(id);
        TempData["Success"] = "Claim moved to Under Review.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        await _claimService.ApproveAsync(id, user!.Id);
        TempData["Success"] = "Claim approved.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        var user = await _userManager.GetUserAsync(User);
        await _claimService.RejectAsync(id, user!.Id, reason);
        TempData["Success"] = "Claim rejected.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordCash(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        await _claimService.RecordCashPayoutAsync(id, user!.Id);
        TempData["Success"] = "Cash payout recorded.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordVoucher(int id, string voucherReference)
    {
        var user = await _userManager.GetUserAsync(User);
        await _claimService.RecordVoucherPayoutAsync(id, user!.Id, voucherReference);
        TempData["Success"] = "Voucher payout recorded.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Download death certificate
    [Authorize(Roles = "Admin,Clerk")]
    public async Task<IActionResult> DownloadCertificate(int id)
    {
        var claim = await _claimService.GetByIdAsync(id);
        if (claim?.DeathCertificateData == null) return NotFound();

        return File(claim.DeathCertificateData,
            "application/octet-stream",
            claim.DeathCertificateFileName ?? "death_certificate");
    }

    private static bool HasBankingDetails(ApplicationUser user)
    {
        return !string.IsNullOrWhiteSpace(user.BankName)
            && !string.IsNullOrWhiteSpace(user.BankAccountName)
            && !string.IsNullOrWhiteSpace(user.BankAccountNumber);
    }
}
