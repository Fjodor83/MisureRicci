using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Services;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace MisureRicci.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuditLogQueryService _auditLogQueryService;
        private readonly IDashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IAuditLogQueryService auditLogQueryService, IDashboardService dashboardService, UserManager<ApplicationUser> userManager)
        {
            _auditLogQueryService = auditLogQueryService;
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole(ApplicationRoles.Admin);
            if (!isAdmin && currentUser?.NegozioId == null)
            {
                return View("TenantAssignmentRequired");
            }

            var model = await _dashboardService.GetKpiAsync(currentUser?.NegozioId, isAdmin);
            return View(model);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TermsOfService()
        {
            return View();
        }

        public IActionResult SecurityStandards()
        {
            return View();
        }

        public IActionResult UserGuide()
        {
            return View();
        }

        public IActionResult TailoringStandards()
        {
            return View();
        }

        public IActionResult TechnicalSupport()
        {
            return View();
        }

        public IActionResult AiutoInfo()
        {
            return View();
        }

        public async Task<IActionResult> ActivityLog(CancellationToken cancellationToken)
        {
            var entries = await _auditLogQueryService.GetLatestAsync(cancellationToken);

            var userIds = entries
                .Select(entry => entry.UserId)
                .Where(userId => !string.IsNullOrWhiteSpace(userId))
                .Distinct()
                .Cast<string>()
                .ToList();

            var userDisplayQuery = _userManager.Users
                .Where(user => userIds.Contains(user.Id))
                .Select(user => new
                {
                    user.Id,
                    user.NomeCompleto,
                    user.UserName
                });

            if (userDisplayQuery.Provider is not IAsyncQueryProvider)
                throw new InvalidOperationException("The query provider does not support async execution.");

            var userDisplayMap = await userDisplayQuery.ToDictionaryAsync(
                user => user.Id,
                user => string.IsNullOrWhiteSpace(user.NomeCompleto)
                    ? (user.UserName ?? user.Id)
                    : user.NomeCompleto,
                cancellationToken);

            ViewBag.UserDisplayMap = userDisplayMap;

            return View(entries);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
