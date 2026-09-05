using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Models;
using SocietyApp.Services.Interfaces;
using SocietyApp.ViewModels;

namespace SocietyApp.Controllers;

[Authorize(Roles = "Organization")]
public class OrganizationController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOrganizationService _organizationService;

    public OrganizationController(UserManager<ApplicationUser> userManager, IOrganizationService organizationService)
    {
        _userManager = userManager;
        _organizationService = organizationService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var organization = await GetOrganizationAsync();
        if (organization == null) return NotFound();

        ViewBag.Members = await _organizationService.GetMembersAsync(organization.Id);
        return View(organization);
    }

    [HttpGet]
    public IActionResult AddMember() => View(new AddOrganizationMemberViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(AddOrganizationMemberViewModel model)
    {
        var organization = await GetOrganizationAsync();
        if (organization == null) return NotFound();
        if (!ModelState.IsValid) return View(model);

        try
        {
            await _organizationService.AddMemberAsync(organization.Id, new OrganizationMember
            {
                FullName = model.FullName.Trim(),
                IDNumber = model.IDNumber.Trim(),
                Phone = model.Phone.Trim(),
                DateOfBirth = model.DateOfBirth
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.IDNumber), ex.Message);
            return View(model);
        }

        TempData["Success"] = "Member added to your organization.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int id)
    {
        var organization = await GetOrganizationAsync();
        if (organization == null) return NotFound();

        var removed = await _organizationService.RemoveMemberAsync(organization.Id, id);
        TempData[removed ? "Success" : "Error"] = removed ? "Member removed." : "Member not found.";
        return RedirectToAction(nameof(Dashboard));
    }

    private async Task<Organization?> GetOrganizationAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user == null ? null : await _organizationService.GetByUserIdAsync(user.Id);
    }
}