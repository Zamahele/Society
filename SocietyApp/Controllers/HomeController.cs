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

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin") || User.IsInRole("Clerk"))
                    return RedirectToAction("Dashboard", "Admin");
                return RedirectToAction("Dashboard", "Members");
            }

            var settings = _dbContext.PublicSiteSettings.FirstOrDefault() ?? new PublicSiteSettings();
            var committee = _dbContext.CommitteeMembers
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Id)
                .ToList();

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
