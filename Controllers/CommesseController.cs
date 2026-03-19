using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize]
    public class CommesseController : Controller
    {
        private readonly ICommessaService _commessaService;
        private readonly IClienteService _clienteService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommesseController(ICommessaService commessaService, IClienteService clienteService, UserManager<ApplicationUser> userManager)
        {
            _commessaService = commessaService;
            _clienteService = clienteService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? clienteId, int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            const int pageSize = 20;
            var result = await _commessaService.GetCommessePagedAsync(clienteId, currentUser?.NegozioId, isAdmin, page, pageSize);
            var kpi = await _commessaService.GetKpiAsync(currentUser?.NegozioId, isAdmin);

            ViewBag.ClienteId = clienteId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);
            ViewBag.Kpi = kpi;

            return View(result.Items);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int clienteId)
        {
            var cliente = await _clienteService.GetClienteByIdAsync(clienteId);
            if (cliente == null)
            {
                return NotFound();
            }

            var vm = new CommessaCreateViewModel
            {
                ClienteId = cliente.Id,
                ClienteNome = $"{cliente.Nome} {cliente.Cognome}".Trim()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommessaCreateViewModel model)
        {
            var cliente = await _clienteService.GetClienteByIdAsync(model.ClienteId);
            if (cliente == null)
            {
                return NotFound();
            }

            model.ClienteNome = $"{cliente.Nome} {cliente.Cognome}".Trim();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var created = await _commessaService.CreateCommessaAsync(model, currentUser?.Id);
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var vm = await _commessaService.GetCommessaDetailsAsync(id, currentUser?.NegozioId, isAdmin);
            if (vm == null)
            {
                return NotFound();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvanceStato(int id, StatoCommessa nuovoStato, string? note)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var ok = await _commessaService.AdvanceStatoAsync(id, nuovoStato, note, currentUser?.Id, currentUser?.NegozioId, isAdmin);
            if (!ok)
            {
                TempData["CommessaError"] = "Transizione stato non consentita o misura non collegata alla commessa.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNota(int id, string nota)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var ok = await _commessaService.AddNotaAsync(id, nota, currentUser?.Id, currentUser?.NegozioId, isAdmin);
            if (!ok)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkMisura(int id, int misuraClienteId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var ok = await _commessaService.LinkMisuraAsync(id, misuraClienteId, currentUser?.Id, currentUser?.NegozioId, isAdmin);
            if (!ok)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlinkMisura(int id, int misuraClienteId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var ok = await _commessaService.UnlinkMisuraAsync(id, misuraClienteId, currentUser?.NegozioId, isAdmin);
            if (!ok)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
