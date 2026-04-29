using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;
using SocietyApp.ViewModels;

namespace SocietyApp.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMembershipService _membershipService;
    private readonly IPaymentService _paymentService;

    public PaymentsController(UserManager<ApplicationUser> userManager,
        IMembershipService membershipService,
        IPaymentService paymentService)
    {
        _userManager = userManager;
        _membershipService = membershipService;
        _paymentService = paymentService;
    }

    // ---- Member: Submit Joining Fee ----

    [Authorize(Roles = "Member")]
    [HttpGet]
    public async Task<IActionResult> SubmitJoiningFee()
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = await _membershipService.GetByUserIdAsync(user!.Id);
        if (membership == null) return NotFound();

        return View(new SubmitJoiningFeeViewModel
        {
            MembershipId = membership.Id,
            MembershipNumber = membership.MembershipNumber
        });
    }

    [Authorize(Roles = "Member")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitJoiningFee(SubmitJoiningFeeViewModel model, string? returnTo = null)
    {
        if (!ModelState.IsValid)
        {
            if (returnTo == "Dashboard") return RedirectToAction("Dashboard", "Members");
            return View(model);
        }

        await _paymentService.SubmitJoiningFeeAsync(model.MembershipId, model.PaymentReference, model.PaymentDate);
        TempData["Success"] = "Joining fee payment submitted. A clerk will confirm it shortly.";
        return RedirectToAction("Dashboard", "Members");
    }

    // ---- Member: Submit Monthly Payment ----

    [Authorize(Roles = "Member")]
    [HttpGet]
    public async Task<IActionResult> SubmitMonthly()
    {
        var user = await _userManager.GetUserAsync(User);
        var membership = await _membershipService.GetByUserIdAsync(user!.Id);
        if (membership == null) return NotFound();

        return View(new SubmitMonthlyPaymentViewModel
        {
            MembershipId = membership.Id,
            MembershipNumber = membership.MembershipNumber
        });
    }

    [Authorize(Roles = "Member")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitMonthly(SubmitMonthlyPaymentViewModel model, string? returnTo = null)
    {
        if (!ModelState.IsValid)
        {
            if (returnTo == "Dashboard") return RedirectToAction("Dashboard", "Members");
            return View(model);
        }

        await _paymentService.SubmitMonthlyPaymentAsync(model.MembershipId, model.ForMonth, model.PaymentReference, model.PaymentDate);
        TempData["Success"] = "Monthly payment submitted. A clerk will confirm it shortly.";
        return RedirectToAction("Dashboard", "Members");
    }

    // ---- Clerk/Admin: Pending Joining Fees ----

    [Authorize(Roles = "Admin,Clerk")]
    public async Task<IActionResult> PendingJoiningFees()
    {
        var list = await _paymentService.GetPendingJoiningFeesAsync();
        return View(list);
    }

    [Authorize(Roles = "Admin,Clerk")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmJoiningFee(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        await _paymentService.ConfirmJoiningFeeAsync(id, user!.Id);
        TempData["Success"] = "Joining fee confirmed. Membership activated.";
        return RedirectToAction(nameof(PendingJoiningFees));
    }

    // ---- Clerk/Admin: Pending Monthly Payments ----

    [Authorize(Roles = "Admin,Clerk")]
    public async Task<IActionResult> PendingMonthly()
    {
        var list = await _paymentService.GetPendingMonthlyPaymentsAsync();
        return View(list);
    }

    [Authorize(Roles = "Admin,Clerk")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmMonthly(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        await _paymentService.ConfirmMonthlyPaymentAsync(id, user!.Id);
        TempData["Success"] = "Monthly payment confirmed.";
        return RedirectToAction(nameof(PendingMonthly));
    }

    [Authorize(Roles = "Admin,Clerk")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJoiningFee(int id)
    {
        await _paymentService.DeleteJoiningFeeAsync(id);
        TempData["Success"] = "Payment record deleted.";
        return RedirectToAction(nameof(PendingJoiningFees));
    }

    [Authorize(Roles = "Admin,Clerk")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMonthly(int id)
    {
        await _paymentService.DeleteMonthlyPaymentAsync(id);
        TempData["Success"] = "Payment record deleted.";
        return RedirectToAction(nameof(PendingMonthly));
    }
}
