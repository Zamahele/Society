using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyApp.Data;
using SocietyApp.Models;
using SocietyApp.ViewModels;

namespace SocietyApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin") || User.IsInRole("Clerk"))
                    return RedirectToAction("Dashboard", "Admin");
                return RedirectToAction("Dashboard", "Members");
            }

            var settings = await _dbContext.PublicSiteSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new PublicSiteSettings();
                _dbContext.PublicSiteSettings.Add(settings);
                await _dbContext.SaveChangesAsync();
            }

            var committee = await _dbContext.CommitteeMembers
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Id)
                .ToListAsync();

            var vm = new PublicLandingViewModel
            {
                Settings = settings,
                CommitteeMembers = committee
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
