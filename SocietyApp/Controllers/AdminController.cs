using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyApp.Data;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;
using SocietyApp.ViewModels;
using System.IO;

namespace SocietyApp.Controllers;

[Authorize(Roles = "Admin,Clerk")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly IMembershipService _membershipService;
    private readonly IPaymentService _paymentService;
    private readonly IClaimService _claimService;

    public AdminController(UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        IMembershipService membershipService,
        IPaymentService paymentService,
        IClaimService claimService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
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

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateMember(int id)
    {
        await _membershipService.SuspendAsync(id);
        TempData["Success"] = "Member deactivated.";
        return RedirectToAction(nameof(MemberDetails), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReactivateMember(int id)
    {
        var membership = await _dbContext.Memberships.FirstOrDefaultAsync(m => m.Id == id);
        if (membership == null)
            return NotFound();

        membership.Status = MembershipStatus.Active;
        membership.DateActivated ??= DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        TempData["Success"] = "Member reactivated.";
        return RedirectToAction(nameof(MemberDetails), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMemberData(int id, string confirmIdNumber)
    {
        var membership = await _dbContext.Memberships
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (membership == null)
            return NotFound();

        if (!string.Equals(confirmIdNumber?.Trim(), membership.User.IDNumber, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Confirmation ID number did not match. Member data was not deleted.";
            return RedirectToAction(nameof(MemberDetails), new { id });
        }

        using var tx = await _dbContext.Database.BeginTransactionAsync();

        _dbContext.DeathClaims.RemoveRange(_dbContext.DeathClaims.Where(c => c.MembershipId == id));
        _dbContext.MonthlyPayments.RemoveRange(_dbContext.MonthlyPayments.Where(p => p.MembershipId == id));
        _dbContext.JoiningFeePayments.RemoveRange(_dbContext.JoiningFeePayments.Where(p => p.MembershipId == id));
        _dbContext.MemberDependants.RemoveRange(_dbContext.MemberDependants.Where(d => d.MembershipId == id));
        _dbContext.Memberships.Remove(membership);
        await _dbContext.SaveChangesAsync();

        var deleteUserResult = await _userManager.DeleteAsync(membership.User);
        if (!deleteUserResult.Succeeded)
        {
            await tx.RollbackAsync();
            TempData["Error"] = "Member records were not deleted. " + string.Join(" ", deleteUserResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(MemberDetails), new { id });
        }

        await tx.CommitAsync();
        TempData["Success"] = "Member and all related records were permanently deleted.";
        return RedirectToAction(nameof(Members));
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

    // ---- Create Member (Admin + Clerk — walk-in at office) ----

    [HttpGet]
    public IActionResult CreateMember() => View(new CreateMemberViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMember(CreateMemberViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.IDNumber,
            IDNumber = model.IDNumber,
            FullName = model.FullName,
            Email = $"{model.IDNumber}@society.local",
            Phone = model.Phone,
            Address = model.Address,
            DateOfBirth = model.DateOfBirth,
            BankAccountName = model.BankAccountName,
            BankAccountNumber = model.BankAccountNumber,
            BankName = model.BankName,
            DateRegistered = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Member");
        await _membershipService.CreateAsync(user.Id);

        TempData["Success"] = $"Member account created for {model.FullName}.";
        return RedirectToAction(nameof(Members));
    }

    // ---- Submit Joining Fee on behalf of member (Admin + Clerk) ----

    [HttpGet]
    public async Task<IActionResult> SubmitJoiningFeeForMember(int membershipId)
    {
        var membership = await _membershipService.GetByIdAsync(membershipId);
        if (membership == null) return NotFound();

        return View(new SubmitJoiningFeeViewModel
        {
            MembershipId = membershipId,
            MembershipNumber = membership.MembershipNumber
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitJoiningFeeForMember(SubmitJoiningFeeViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var clerk = await _userManager.GetUserAsync(User);
        await _paymentService.SubmitJoiningFeeAsync(model.MembershipId, model.PaymentReference, model.PaymentDate, clerk!.Id);
        TempData["Success"] = "Joining fee submitted on behalf of member.";
        return RedirectToAction(nameof(MemberDetails), new { id = model.MembershipId });
    }

    // ---- Submit Monthly Payment on behalf of member (Admin + Clerk) ----

    [HttpGet]
    public async Task<IActionResult> SubmitMonthlyForMember(int membershipId)
    {
        var membership = await _membershipService.GetByIdAsync(membershipId);
        if (membership == null) return NotFound();

        return View(new SubmitMonthlyPaymentViewModel
        {
            MembershipId = membershipId,
            MembershipNumber = membership.MembershipNumber
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitMonthlyForMember(SubmitMonthlyPaymentViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var clerk = await _userManager.GetUserAsync(User);
        await _paymentService.SubmitMonthlyPaymentAsync(model.MembershipId, model.ForMonth, model.PaymentReference, model.PaymentDate, clerk!.Id);
        TempData["Success"] = "Monthly payment submitted on behalf of member.";
        return RedirectToAction(nameof(MemberDetails), new { id = model.MembershipId });
    }

    // ---- Submit Claim on behalf of member (Admin + Clerk) ----

    [HttpGet]
    public async Task<IActionResult> SubmitClaimForMember(int membershipId)
    {
        var membership = await _membershipService.GetByIdAsync(membershipId);
        if (membership == null) return NotFound();

        var eligibility = await _claimService.CheckEligibilityAsync(membershipId);
        if (!eligibility.IsEligible)
        {
            ViewBag.Reasons = eligibility.Reasons;
            return View("NotEligible");
        }

        var dependants = await _membershipService.GetDependantsAsync(membershipId);
        return View(new SubmitClaimViewModel
        {
            MembershipId = membershipId,
            Dependants = dependants
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitClaimForMember(SubmitClaimViewModel model)
    {
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

        var clerk = await _userManager.GetUserAsync(User);
        await _claimService.SubmitClaimAsync(model.MembershipId, claim, certData, certFileName, clerk!.Id);
        TempData["Success"] = "Claim submitted on behalf of member.";
        return RedirectToAction(nameof(MemberDetails), new { id = model.MembershipId });
    }

    [HttpGet]
    public async Task<IActionResult> PublicContent()
    {
        var settings = await _dbContext.PublicSiteSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new PublicSiteSettings();
            _dbContext.PublicSiteSettings.Add(settings);
            await _dbContext.SaveChangesAsync();
        }

        var model = new PublicContentAdminViewModel
        {
            Settings = settings,
            CommitteeMembers = await _dbContext.CommitteeMembers
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Id)
                .ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublicContent(PublicContentAdminViewModel model)
    {
        var settings = await _dbContext.PublicSiteSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = model.Settings;
            _dbContext.PublicSiteSettings.Add(settings);
        }
        else
        {
            settings.OrganizationName = model.Settings.OrganizationName;
            settings.RegistrationNumber = model.Settings.RegistrationNumber;
            settings.EnterpriseType = model.Settings.EnterpriseType;
            settings.EnterpriseStatus = model.Settings.EnterpriseStatus;
            settings.RegistrationDate = model.Settings.RegistrationDate;
            settings.BusinessStartDate = model.Settings.BusinessStartDate;
            settings.FinancialYearEnd = model.Settings.FinancialYearEnd;
            settings.MainBusinessObject = model.Settings.MainBusinessObject;
            settings.PostalAddress = model.Settings.PostalAddress;
            settings.RegisteredOfficeAddress = model.Settings.RegisteredOfficeAddress;
            settings.BankName = model.Settings.BankName;
            settings.BankAccountName = model.Settings.BankAccountName;
            settings.BankAccountNumber = model.Settings.BankAccountNumber;
            settings.BankBranchCode = model.Settings.BankBranchCode;
            settings.BankAccountType = model.Settings.BankAccountType;
            settings.ContactAddress = model.Settings.ContactAddress;
            settings.ContactPhone1 = model.Settings.ContactPhone1;
            settings.ContactPhone2 = model.Settings.ContactPhone2;
            settings.ContactPhone3 = model.Settings.ContactPhone3;
            settings.ContactEmailInfo = model.Settings.ContactEmailInfo;
            settings.ContactEmailClaims = model.Settings.ContactEmailClaims;
        }

        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Public site content updated.";
        return RedirectToAction(nameof(PublicContent));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCommitteeMember(PublicContentAdminViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.NewMemberName) || string.IsNullOrWhiteSpace(model.NewMemberRole))
        {
            TempData["Error"] = "Member name and role are required.";
            return RedirectToAction(nameof(PublicContent));
        }

        var maxOrder = await _dbContext.CommitteeMembers.Select(c => (int?)c.DisplayOrder).MaxAsync() ?? 0;
        _dbContext.CommitteeMembers.Add(new CommitteeMember
        {
            FullName = model.NewMemberName.Trim(),
            RoleTitle = model.NewMemberRole.Trim(),
            Phone = model.NewMemberPhone?.Trim() ?? string.Empty,
            DisplayOrder = maxOrder + 1,
            IsActive = true
        });

        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Committee member added.";
        return RedirectToAction(nameof(PublicContent));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCommitteeMember(int id)
    {
        var member = await _dbContext.CommitteeMembers.FirstOrDefaultAsync(c => c.Id == id);
        if (member != null)
        {
            _dbContext.CommitteeMembers.Remove(member);
            await _dbContext.SaveChangesAsync();
            TempData["Success"] = "Committee member removed.";
        }

        return RedirectToAction(nameof(PublicContent));
    }
}
