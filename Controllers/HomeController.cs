using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;
using System.Diagnostics;

namespace MisureRicci.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Data.ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, Data.ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalClients = await _context.Clienti.CountAsync();
            ViewBag.TotalStores = await _context.Negozi.CountAsync();
            ViewBag.TotalStaff = await _context.Utenti.CountAsync();

            // Registry is the single source of truth for both legacy and dynamic measurements.
            ViewBag.TotalMeasurements = await _context.RegistroMisure.CountAsync();

            return View();
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
