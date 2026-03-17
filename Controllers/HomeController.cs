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
            
            // Total measurements count across all tables
            ViewBag.TotalMeasurements = 
                await _context.MisureGiacca.CountAsync() +
                await _context.MisurePantalone.CountAsync() +
                await _context.MisureCamicia.CountAsync() +
                await _context.MisureAbitoCompleto.CountAsync();

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
