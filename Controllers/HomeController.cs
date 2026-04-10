using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Services;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace MisureRicci.Controllers
{
    public class HomeController(IAuditLogQueryService auditLogQueryService, IDashboardService dashboardService, UserManager<ApplicationUser> userManager) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var currentUser = await userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole(ApplicationRoles.Admin);
            if (!isAdmin && currentUser?.NegozioId == null)
            {
                return View("TenantAssignmentRequired");
            }

            var model = await dashboardService.GetKpiAsync(currentUser?.NegozioId, isAdmin);
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
            var entries = await auditLogQueryService.GetLatestAsync(cancellationToken);

            var userIds = entries
                .Select(entry => entry.UserId)
                .Where(userId => !string.IsNullOrWhiteSpace(userId))
                .Distinct()
                .ToList();

            var userDisplayQuery = userManager.Users
                .Where(user => userIds.Contains(user.Id))
                .Select(user => new UserDisplayProjection(
                    user.Id,
                    user.NomeCompleto,
                    user.UserName));

            var userDisplayMap = await BuildUserDisplayMapAsync(userDisplayQuery, cancellationToken);

            ViewBag.UserDisplayMap = userDisplayMap;

            return View(entries);
        }

        private static Task<Dictionary<string, string>> BuildUserDisplayMapAsync(
            IQueryable<UserDisplayProjection> userDisplayQuery,
            CancellationToken cancellationToken)
        {
            if (userDisplayQuery.Provider is IAsyncQueryProvider)
            {
                return userDisplayQuery.ToDictionaryAsync(
                    user => user.Id,
                    user => string.IsNullOrWhiteSpace(user.NomeCompleto)
                        ? (user.UserName ?? user.Id)
                        : user.NomeCompleto,
                    cancellationToken);
            }

            var map = userDisplayQuery.ToDictionary(
                user => user.Id,
                user => string.IsNullOrWhiteSpace(user.NomeCompleto)
                    ? (user.UserName ?? user.Id)
                    : user.NomeCompleto);

            return Task.FromResult(map);
        }

        private sealed record UserDisplayProjection(string Id, string? NomeCompleto, string? UserName);

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
