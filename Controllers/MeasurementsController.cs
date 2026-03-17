using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                await _context.SaveChangesAsync();
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Giacca", 
                    Note = "Nuova misura giacca registrata",
                    RecordId = model.Id
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
                await _context.SaveChangesAsync();
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Pantalone", 
                    Note = "Nuova misura pantalone registrata",
                    RecordId = model.Id
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
                await _context.SaveChangesAsync();
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Gilet", 
                    Note = "Nuova misura gilet registrata",
                    RecordId = model.Id
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
                await _context.SaveChangesAsync();
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Maglie", 
                    Note = "Nuova misura maglieria registrata",
                    RecordId = model.Id
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
                await _context.SaveChangesAsync();
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Outdoor", 
                    Note = "Nuova misura outdoor registrata",
                    RecordId = model.Id
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

                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Abito", 
                    Note = "Nuova misura abito completo registrata",
                    RecordId = model.Id
                });
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
                await _context.SaveChangesAsync();
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Camicia", 
                    Note = "Nuova misura camicia registrata",
                    RecordId = model.Id
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
                await _context.SaveChangesAsync();
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Scarpe", 
                    Note = "Nuova misura scarpe registrata",
                    RecordId = model.Id
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
                await _context.SaveChangesAsync();
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Cravatta", 
                    Note = "Nuova misura cravatta registrata",
                    RecordId = model.Id
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
                await _context.SaveChangesAsync();
                _context.RegistroMisure.Add(new MisureCliente { 
                    ClienteId = model.ClienteId, 
                    TipoMisura = "Cintura", 
                    Note = "Nuova misura cintura registrata",
                    RecordId = model.Id
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
            }
            return View(model);
        }

        private async Task<object?> GetMeasurementAsync(int id, string tipoMisura)
        {
            return tipoMisura.ToLower() switch
            {
                "giacca" => await _context.MisureGiacca.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "pantalone" => await _context.MisurePantalone.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "abito" => await _context.MisureAbitoCompleto.Include(m => m.Cliente).Include(m => m.Giacca).Include(m => m.Pantalone).FirstOrDefaultAsync(m => m.Id == id),
                "gilet" => await _context.MisureGilet.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "maglie" => await _context.MisureMaglie.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "outdoor" => await _context.MisureOutdoor.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "camicia" => await _context.MisureCamicia.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "scarpe" => await _context.MisureScarpe.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "cravatta" => await _context.MisureCravatta.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "cintura" => await _context.MisureCintura.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                _ => null
            };
        }

        public async Task<IActionResult> Details(int? id, string tipoMisura)
        {
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();
            var model = await GetMeasurementAsync(id.Value, tipoMisura);
            if (model == null) return NotFound();
            ViewBag.TipoMisura = tipoMisura;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string tipoMisura)
        {
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();
            var model = await GetMeasurementAsync(id.Value, tipoMisura);
            if (model == null) return NotFound();
            ViewBag.TipoMisura = tipoMisura;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string tipoMisura, Microsoft.AspNetCore.Http.IFormCollection form)
        {
            if (string.IsNullOrEmpty(tipoMisura)) return NotFound();
            var model = await GetMeasurementAsync(id, tipoMisura);
            if (model == null) return NotFound();

            if (await TryUpdateModelAsync(model, model.GetType(), "")) 
            {
                try
                {
                    await _context.SaveChangesAsync();
                    int clienteId = (int)model.GetType().GetProperty("ClienteId").GetValue(model);
                    return RedirectToAction("Details", "Clienti", new { id = clienteId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }
            ViewBag.TipoMisura = tipoMisura;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id, string tipoMisura)
        {
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();
            var model = await GetMeasurementAsync(id.Value, tipoMisura);
            if (model == null) return NotFound();
            ViewBag.TipoMisura = tipoMisura;
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string tipoMisura)
        {
            if (string.IsNullOrEmpty(tipoMisura)) return NotFound();
            var model = await GetMeasurementAsync(id, tipoMisura);
            if (model != null)
            {
                int clienteId = (int)model.GetType().GetProperty("ClienteId").GetValue(model);
                _context.Remove(model);
                
                var registro = await _context.RegistroMisure.FirstOrDefaultAsync(r => r.RecordId == id && r.TipoMisura.ToLower() == tipoMisura.ToLower());
                if (registro != null)
                {
                    _context.RegistroMisure.Remove(registro);
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Clienti", new { id = clienteId });
            }
            return RedirectToAction(nameof(Index), "Clienti");
        }
    }
}

