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

    // ---- Submit Joining Fee (Member self-submit; Admin/Clerk on behalf) ----

    [Authorize(Roles = "Member,Admin,Clerk")]
    [HttpGet]
    public async Task<IActionResult> SubmitJoiningFee(int? membershipId = null)
    {
        var user = await _userManager.GetUserAsync(User);
        var isStaff = User.IsInRole("Admin") || User.IsInRole("Clerk");

        Membership? membership = isStaff && membershipId.HasValue
            ? await _membershipService.GetByIdAsync(membershipId.Value)
            : await _membershipService.GetByUserIdAsync(user!.Id);

        if (membership == null) return NotFound();

        ViewBag.IsStaffSubmit = isStaff && membershipId.HasValue;
        return View(new SubmitJoiningFeeViewModel
        {
            MembershipId = membership.Id,
            MembershipNumber = membership.MembershipNumber
        });
    }

    [Authorize(Roles = "Member,Admin,Clerk")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitJoiningFee(SubmitJoiningFeeViewModel model, string? returnTo = null)
    {
        var user = await _userManager.GetUserAsync(User);
        var isStaff = User.IsInRole("Admin") || User.IsInRole("Clerk");

        if (!ModelState.IsValid)
        {
            if (returnTo == "Dashboard") return RedirectToAction("Dashboard", "Members");
            ViewBag.IsStaffSubmit = isStaff;
            return View(model);
        }

        if (await _paymentService.HasPendingJoiningFeeAsync(model.MembershipId))
        {
            TempData["Error"] = "A joining fee submission is already pending confirmation. Please wait for a clerk to review it.";
            return isStaff
                ? RedirectToAction("MemberDetails", "Admin", new { id = model.MembershipId })
                : RedirectToAction("Dashboard", "Members");
        }

        await _paymentService.SubmitJoiningFeeAsync(model.MembershipId, model.PaymentReference, model.PaymentDate, isStaff ? user!.Id : null);
        TempData["Success"] = "Joining fee payment submitted. A clerk will confirm it shortly.";
        return isStaff
            ? RedirectToAction("MemberDetails", "Admin", new { id = model.MembershipId })
            : RedirectToAction("Dashboard", "Members");
    }

    // ---- Submit Monthly Payment (Member self-submit; Admin/Clerk on behalf) ----

    [Authorize(Roles = "Member,Admin,Clerk")]
    [HttpGet]
    public async Task<IActionResult> SubmitMonthly(int? membershipId = null)
    {
        var user = await _userManager.GetUserAsync(User);
        var isStaff = User.IsInRole("Admin") || User.IsInRole("Clerk");

        Membership? membership = isStaff && membershipId.HasValue
            ? await _membershipService.GetByIdAsync(membershipId.Value)
            : await _membershipService.GetByUserIdAsync(user!.Id);

        if (membership == null) return NotFound();

        ViewBag.IsStaffSubmit = isStaff && membershipId.HasValue;
        return View(new SubmitMonthlyPaymentViewModel
        {
            MembershipId = membership.Id,
            MembershipNumber = membership.MembershipNumber
        });
    }

    [Authorize(Roles = "Member,Admin,Clerk")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitMonthly(SubmitMonthlyPaymentViewModel model, string? returnTo = null)
    {
        var user = await _userManager.GetUserAsync(User);
        var isStaff = User.IsInRole("Admin") || User.IsInRole("Clerk");

        if (!ModelState.IsValid)
        {
            if (returnTo == "Dashboard") return RedirectToAction("Dashboard", "Members");
            ViewBag.IsStaffSubmit = isStaff;
            return View(model);
        }

        await _paymentService.SubmitMonthlyPaymentAsync(model.MembershipId, model.ForMonth, model.PaymentReference, model.PaymentDate, isStaff ? user!.Id : null);
        TempData["Success"] = "Monthly payment submitted. A clerk will confirm it shortly.";
        return isStaff
            ? RedirectToAction("MemberDetails", "Admin", new { id = model.MembershipId })
            : RedirectToAction("Dashboard", "Members");
    }

    // ---- Member: Unified Submit Payment ----

    [Authorize(Roles = "Member")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitPayment(SubmitPaymentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in all required fields.";
            return RedirectToAction("Dashboard", "Members");
        }

        byte[]? proofData = null;
        string? proofFileName = null;
        if (model.Proof != null && model.Proof.Length > 0)
        {
            using var ms = new MemoryStream();
            await model.Proof.CopyToAsync(ms);
            proofData = ms.ToArray();
            proofFileName = model.Proof.FileName;
        }

        var paymentDate = DateTime.Now;

        if (model.PaymentType == "JoiningFee")
        {
            if (await _paymentService.HasPendingJoiningFeeAsync(model.MembershipId))
            {
                TempData["Error"] = "A joining fee submission is already pending confirmation.";
                return RedirectToAction("Dashboard", "Members");
            }
            await _paymentService.SubmitJoiningFeeAsync(model.MembershipId, model.PaymentReference, paymentDate, proofData: proofData, proofFileName: proofFileName);
            TempData["Success"] = "Joining fee submitted. A clerk will confirm it shortly.";
        }
        else if (model.PaymentType == "Monthly" && model.ForMonth.HasValue)
        {
            await _paymentService.SubmitMonthlyPaymentAsync(model.MembershipId, model.ForMonth.Value, model.PaymentReference, paymentDate, proofData: proofData, proofFileName: proofFileName);
            TempData["Success"] = "Monthly payment submitted. A clerk will confirm it shortly.";
        }
        else
        {
            TempData["Error"] = "Invalid payment type.";
        }

        return RedirectToAction("Dashboard", "Members");
    }

    [Authorize(Roles = "Admin,Clerk")]
    [HttpGet]
    public async Task<IActionResult> JoiningFeeProof(int id)
    {
        var payment = await _paymentService.GetJoiningFeeByIdAsync(id);
        if (payment?.ProofData == null) return NotFound();
        return File(payment.ProofData, GetContentType(payment.ProofFileName));
    }

    [Authorize(Roles = "Admin,Clerk")]
    [HttpGet]
    public async Task<IActionResult> MonthlyProof(int id)
    {
        var payment = await _paymentService.GetMonthlyPaymentByIdAsync(id);
        if (payment?.ProofData == null) return NotFound();
        return File(payment.ProofData, GetContentType(payment.ProofFileName));
    }

    private static string GetContentType(string? fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".pdf"  => "application/pdf",
            ".png"  => "image/png",
            ".gif"  => "image/gif",
            ".webp" => "image/webp",
            _       => "image/jpeg"
        };
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
        TempData["Success"] = "Joining fee marked as confirmed. To activate the membership, use Approve Member on the member's detail page.";
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
