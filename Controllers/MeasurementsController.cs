using Microsoft.AspNetCore.Mvc;
using MisureRicci.Data;
using MisureRicci.Models;
using System.Threading.Tasks;

namespace MisureRicci.Controllers
{
    public class MeasurementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MeasurementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? clienteId)
        {
            if (clienteId == null) return RedirectToAction("Index", "Clienti");

            var cliente = await _context.Clienti.FindAsync(clienteId);
            if (cliente == null) return NotFound();

            ViewBag.ClienteId = clienteId;
            ViewBag.ClienteNome = $"{cliente.Nome} {cliente.Cognome}";
            return View();
        }

        [HttpGet]
        public IActionResult CreateGiacca(int clienteId) => View(new GiaccaMeasurement { ClienteId = clienteId });

        [HttpPost]
        public async Task<IActionResult> CreateGiacca(GiaccaMeasurement model)
        {
            if (ModelState.IsValid)
            {
                _context.MisureGiacca.Add(model);
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Giacca", 
                    Note = "Nuova misura giacca registrata" 
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CreatePantalone(int clienteId) => View(new PantaloneMeasurement { ClienteId = clienteId });

        [HttpPost]
        public async Task<IActionResult> CreatePantalone(PantaloneMeasurement model)
        {
            if (ModelState.IsValid)
            {
                _context.MisurePantalone.Add(model);
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Pantalone", 
                    Note = "Nuova misura pantalone registrata" 
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateGilet(int clienteId) => View(new GiletMeasurement { ClienteId = clienteId });

        [HttpPost]
        public async Task<IActionResult> CreateGilet(GiletMeasurement model)
        {
            if (ModelState.IsValid)
            {
                _context.MisureGilet.Add(model);
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Gilet", 
                    Note = "Nuova misura gilet registrata" 
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateMaglie(int clienteId) => View(new MaglieMeasurement { ClienteId = clienteId });

        [HttpPost]
        public async Task<IActionResult> CreateMaglie(MaglieMeasurement model)
        {
            if (ModelState.IsValid)
            {
                _context.MisureMaglie.Add(model);
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Maglie", 
                    Note = "Nuova misura maglieria registrata" 
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateOutdoor(int clienteId) => View(new OutdoorMeasurement { ClienteId = clienteId });

        [HttpPost]
        public async Task<IActionResult> CreateOutdoor(OutdoorMeasurement model)
        {
            if (ModelState.IsValid)
            {
                _context.MisureOutdoor.Add(model);
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Outdoor", 
                    Note = "Nuova misura outdoor registrata" 
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateAbito(int clienteId) => View(new AbitoCompletoMeasurement { ClienteId = clienteId });

        [HttpPost]
        public async Task<IActionResult> CreateAbito(AbitoCompletoMeasurement model)
        {
            // Abito Completo specific logic might need to ensure ClienteId is set for sub-parts if handled separately
            if (ModelState.IsValid)
            {
                // Assuming Giacca and Pantalone are navigation properties and need ClienteId set if they are new entities
                // If they are part of the AbitoCompleto model and their ClienteId is automatically cascaded, this might not be strictly necessary,
                // but it's good practice to ensure consistency.
                if (model.Giacca != null)
                {
                    model.Giacca.ClienteId = model.ClienteId;
                }
                if (model.Pantalone != null)
                {
                    model.Pantalone.ClienteId = model.ClienteId;
                }
                
                _context.MisureAbitoCompleto.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateCamicia(int clienteId) => View(new CamiciaMeasurement { ClienteId = clienteId });

        [HttpPost]
        public async Task<IActionResult> CreateCamicia(CamiciaMeasurement model)
        {
            if (ModelState.IsValid)
            {
                _context.MisureCamicia.Add(model);
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Camicia", 
                    Note = "Nuova misura camicia registrata" 
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateScarpe(int clienteId) => View(new ScarpeMeasurement { ClienteId = clienteId });

        [HttpPost]
        public async Task<IActionResult> CreateScarpe(ScarpeMeasurement model)
        {
            if (ModelState.IsValid)
            {
                _context.MisureScarpe.Add(model);
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Scarpe", 
                    Note = "Nuova misura scarpe registrata" 
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateCravatta(int clienteId) => View(new CravattaMeasurement { ClienteId = clienteId });

        [HttpPost]
        public async Task<IActionResult> CreateCravatta(CravattaMeasurement model)
        {
            if (ModelState.IsValid)
            {
                _context.MisureCravatta.Add(model);
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Cravatta", 
                    Note = "Nuova misura cravatta registrata" 
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateCintura(int clienteId) => View(new CinturaMeasurement { ClienteId = clienteId });

        [HttpPost]
        public async Task<IActionResult> CreateCintura(CinturaMeasurement model)
        {
            if (ModelState.IsValid)
            {
                _context.MisureCintura.Add(model);
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Cintura", 
                    Note = "Nuova misura cintura registrata" 
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
            }
            return View(model);
        }
    }
}

