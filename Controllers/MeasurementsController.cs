using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Controllers
{
    public class MeasurementsController : Controller
    {
        private readonly IMeasurementService _measurementService;
        private readonly IClienteService _clienteService;
        private readonly ICustomMeasurementService _customMeasurementService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MeasurementsController(IMeasurementService measurementService, IClienteService clienteService, ICustomMeasurementService customMeasurementService, UserManager<ApplicationUser> userManager)
        {
            _measurementService = measurementService;
            _clienteService = clienteService;
            _customMeasurementService = customMeasurementService;
            _userManager = userManager;
        }

        public async Task<IActionResult> GlobalRegistry(string filter, int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            const int pageSize = 20;

            var result = await _measurementService.GetGlobalRegistryPagedAsync(filter, currentUser?.NegozioId, isAdmin, page, pageSize);

            ViewBag.Categoria = string.IsNullOrEmpty(filter) ? "TUTTE LE CATEGORIE" : filter.ToUpper();
            ViewBag.Filter = filter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling(result.TotalCount / (double)pageSize);
            
            return View(result.Items);
        }

        public async Task<IActionResult> Index(int? clienteId)
        {
            if (clienteId == null) return RedirectToAction("Index", "Clienti");

            var cliente = await _clienteService.GetClienteByIdAsync(clienteId.Value);
            if (cliente == null) return NotFound();

            ViewBag.ClienteId = clienteId;
            ViewBag.ClienteNome = $"{cliente.Nome} {cliente.Cognome}";
            ViewBag.DynamicMeasurementTypes = await _customMeasurementService.GetMeasurementTypesAsync(onlyActive: true);
            return View();
        }

        public async Task<IActionResult> Details(int? id, string tipoMisura, int? registryId)
        {
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();

            var resolved = await ResolveMeasurementDisplayAsync(id.Value, tipoMisura, registryId);
            if (resolved.DynamicRecordId.HasValue)
            {
                return RedirectToAction("Details", "DynamicMeasurements", new { id = resolved.DynamicRecordId.Value });
            }

            if (resolved.Model == null)
            {
                return NotFound();
            }

            return View(BuildDetailsViewModel(resolved.Model, resolved.TipoMisura));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string tipoMisura)
        {
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            var model = await _measurementService.GetMeasurementScopedAsync(id.Value, tipoMisura, currentUser?.NegozioId, isAdmin);
            if (model == null) return NotFound();

            return View(BuildEditViewModel(model, tipoMisura));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LegacyMeasurementEditViewModel input)
        {
            if (string.IsNullOrEmpty(input.TipoMisura)) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            var model = await _measurementService.GetMeasurementScopedAsync(input.Id, input.TipoMisura, currentUser?.NegozioId, isAdmin);
            if (model == null) return NotFound();

            if (TryApplyEditableMeasurementFields(model, input.Fields) && TryValidateModel(model))
            {
                if (await _measurementService.UpdateMeasurementAsync(model, input.TipoMisura))
                {
                    int clienteId = GetClienteId(model);
                    return RedirectToAction("Details", "Clienti", new { id = clienteId });
                }

                return NotFound();
            }

            return View(BuildEditViewModel(model, input.TipoMisura, input.Fields));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id, string tipoMisura, int? registryId)
        {
            if (id == null || string.IsNullOrEmpty(tipoMisura)) return NotFound();

            var resolved = await ResolveMeasurementDisplayAsync(id.Value, tipoMisura, registryId);
            if (resolved.DynamicRecordId.HasValue)
            {
                return RedirectToAction("Details", "DynamicMeasurements", new { id = resolved.DynamicRecordId.Value });
            }

            if (resolved.Model == null)
            {
                return NotFound();
            }

            return View(BuildDeleteViewModel(resolved.Model, resolved.TipoMisura, resolved.RegistryId));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string tipoMisura, int? registryId)
        {
            if (string.IsNullOrEmpty(tipoMisura)) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            if (registryId.HasValue)
            {
                var clienteIdFromRegistry = await _measurementService.DeleteByRegistryEntryAsync(registryId.Value, currentUser?.NegozioId, isAdmin);
                if (clienteIdFromRegistry.HasValue)
                {
                    return RedirectToAction("Details", "Clienti", new { id = clienteIdFromRegistry.Value });
                }

                return RedirectToAction(nameof(Index), "Clienti");
            }

            var model = await _measurementService.GetMeasurementScopedAsync(id, tipoMisura, currentUser?.NegozioId, isAdmin);
            if (model != null)
            {
                int clienteId = GetClienteId(model);
                await _measurementService.DeleteMeasurementAsync(id, tipoMisura);
                return RedirectToAction("Details", "Clienti", new { id = clienteId });
            }
            return RedirectToAction(nameof(Index), "Clienti");
        }

        private int GetClienteId(object model)
        {
            return (int)(model.GetType().GetProperty("ClienteId")?.GetValue(model)
                ?? throw new InvalidOperationException("ClienteId non disponibile per la misura richiesta."));
        }

        private bool TryApplyEditableMeasurementFields(object model, IEnumerable<LegacyMeasurementFieldViewModel> fields)
        {
            var valuesByName = fields.ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

            foreach (var property in GetEditableMeasurementProperties(model.GetType()))
            {
                if (!valuesByName.TryGetValue(property.Name, out var rawValue))
                {
                    continue;
                }

                if (!TryConvertFormValue(property.PropertyType, rawValue, out var convertedValue))
                {
                    ModelState.AddModelError(property.Name, $"Valore non valido per il campo {property.Name}.");
                    continue;
                }

                property.SetValue(model, convertedValue);
            }

            return ModelState.IsValid;
        }

        private LegacyMeasurementEditViewModel BuildEditViewModel(object model, string tipoMisura, IEnumerable<LegacyMeasurementFieldViewModel>? postedFields = null)
        {
            var fields = BuildFieldViewModels(model).ToList();
            var postedValues = postedFields?.ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

            if (postedValues != null)
            {
                foreach (var field in fields)
                {
                    if (postedValues.TryGetValue(field.Name, out var postedValue))
                    {
                        field.Value = postedValue;
                    }
                }
            }

            var canEditFields = !string.Equals(tipoMisura, "abito", StringComparison.OrdinalIgnoreCase);

            return new LegacyMeasurementEditViewModel
            {
                Id = GetMeasurementId(model),
                ClienteId = GetClienteId(model),
                TipoMisura = tipoMisura,
                Fields = fields,
                CanEditFields = canEditFields,
                WarningMessage = canEditFields
                    ? null
                    : "Modifica per Abito Completo non disponibile da questa vista rapida. Usa workflow dedicato o API specifiche."
            };
        }

        private LegacyMeasurementDetailsViewModel BuildDetailsViewModel(object model, string tipoMisura)
        {
            var measurementType = model.GetType();
            var cliente = measurementType.GetProperty("Cliente")?.GetValue(model) as Cliente;
            var details = new LegacyMeasurementDetailsViewModel
            {
                Id = GetMeasurementId(model),
                ClienteId = GetClienteId(model),
                TipoMisura = tipoMisura,
                ClienteNome = cliente == null ? string.Empty : $"{cliente.Nome} {cliente.Cognome}".Trim(),
                CreatedAt = (DateTime)(measurementType.GetProperty("CreatedAt")?.GetValue(model) ?? DateTime.MinValue),
                Notes = measurementType.GetProperty("Notes")?.GetValue(model) as string,
                Fields = BuildDisplayFields(model).ToList()
            };

            if (string.Equals(tipoMisura, "abito", StringComparison.OrdinalIgnoreCase))
            {
                var giacca = measurementType.GetProperty("Giacca")?.GetValue(model);
                var pantalone = measurementType.GetProperty("Pantalone")?.GetValue(model);

                if (giacca != null)
                {
                    details.Sections.Add(new LegacyMeasurementSectionViewModel
                    {
                        Title = "Giacca",
                        Fields = BuildDisplayFields(giacca).ToList()
                    });
                }

                if (pantalone != null)
                {
                    details.Sections.Add(new LegacyMeasurementSectionViewModel
                    {
                        Title = "Pantalone",
                        Fields = BuildDisplayFields(pantalone).ToList()
                    });
                }

                details.Fields.Clear();
            }

            return details;
        }

        private LegacyMeasurementDeleteViewModel BuildDeleteViewModel(object model, string tipoMisura, int? registryId)
        {
            return new LegacyMeasurementDeleteViewModel
            {
                Id = GetMeasurementId(model),
                ClienteId = GetClienteId(model),
                TipoMisura = tipoMisura,
                RegistryId = registryId
            };
        }

        private static int GetMeasurementId(object model)
        {
            return (int)(model.GetType().GetProperty("Id")?.GetValue(model)
                ?? throw new InvalidOperationException("Id non disponibile per la misura richiesta."));
        }

        private static IEnumerable<LegacyMeasurementFieldViewModel> BuildFieldViewModels(object model)
        {
            return GetEditableMeasurementProperties(model.GetType())
                .Select(property => new LegacyMeasurementFieldViewModel
                {
                    Name = property.Name,
                    DisplayName = property.GetCustomAttribute<DisplayAttribute>()?.Name ?? property.Name,
                    Value = ConvertToDisplayValue(property.GetValue(model)),
                    IsMultiline = property.Name == "Notes"
                });
        }

        private static IEnumerable<LegacyMeasurementFieldViewModel> BuildDisplayFields(object model)
        {
            return model.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != "Id" && p.Name != "ClienteId" && p.Name != "Cliente" && p.Name != "CreatedAt" && p.Name != "OrderId" && p.Name != "Notes" && p.Name != "Giacca" && p.Name != "Pantalone")
                .Select(property => new LegacyMeasurementFieldViewModel
                {
                    Name = property.Name,
                    DisplayName = property.GetCustomAttribute<DisplayAttribute>()?.Name ?? property.Name,
                    Value = ConvertToDisplayValue(property.GetValue(model))
                });
        }

        private static string? ConvertToDisplayValue(object? value)
        {
            return value switch
            {
                null => null,
                DateTime dateTime => dateTime.ToString("f", CultureInfo.CurrentCulture),
                IFormattable formattable => formattable.ToString(null, CultureInfo.CurrentCulture),
                _ => value.ToString()
            };
        }

        private static IEnumerable<PropertyInfo> GetEditableMeasurementProperties(Type type)
        {
            return type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .Where(p => p.Name != "Id"
                    && p.Name != "ClienteId"
                    && p.Name != "Cliente"
                    && p.Name != "CreatedAt"
                    && p.Name != "OrderId"
                    && p.Name != "Giacca"
                    && p.Name != "Pantalone");
        }

        private static bool TryConvertFormValue(Type propertyType, string? rawValue, out object? convertedValue)
        {
            var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (targetType == typeof(string))
            {
                convertedValue = rawValue?.Trim();
                return true;
            }

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                convertedValue = Nullable.GetUnderlyingType(propertyType) != null ? null : Activator.CreateInstance(targetType);
                return true;
            }

            if (targetType == typeof(double))
            {
                if (double.TryParse(rawValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var currentValue)
                    || double.TryParse(rawValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out currentValue))
                {
                    convertedValue = currentValue;
                    return true;
                }

                convertedValue = null;
                return false;
            }

            if (targetType == typeof(int))
            {
                if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.CurrentCulture, out var intValue))
                {
                    convertedValue = intValue;
                    return true;
                }

                convertedValue = null;
                return false;
            }

            if (targetType == typeof(bool))
            {
                if (bool.TryParse(rawValue, out var boolValue))
                {
                    convertedValue = boolValue;
                    return true;
                }

                convertedValue = null;
                return false;
            }

            convertedValue = null;
            return false;
        }

        private async Task<(object? Model, string TipoMisura, int? RegistryId, int? DynamicRecordId)> ResolveMeasurementDisplayAsync(int id, string tipoMisura, int? registryId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            if (registryId.HasValue)
            {
                var registryEntry = await _measurementService.GetRegistryEntryAsync(registryId.Value, currentUser?.NegozioId, isAdmin);
                if (registryEntry == null)
                {
                    return (null, tipoMisura, null, null);
                }

                if (registryEntry.IsDynamic)
                {
                    return (null, registryEntry.TipoMisura, registryEntry.Id, registryEntry.RecordId);
                }

                var legacyModel = await _measurementService.GetMeasurementByRegistryEntryAsync(registryId.Value, currentUser?.NegozioId, isAdmin);
                return (legacyModel, registryEntry.TipoMisura, registryEntry.Id, null);
            }

            var model = await _measurementService.GetMeasurementScopedAsync(id, tipoMisura, currentUser?.NegozioId, isAdmin);
            if (model != null)
            {
                return (model, tipoMisura, registryId, null);
            }

            var dynamicType = (await _customMeasurementService.GetMeasurementTypesAsync(onlyActive: false))
                .FirstOrDefault(x => string.Equals(x.Nome, tipoMisura, StringComparison.OrdinalIgnoreCase));

            if (dynamicType != null)
            {
                return (null, tipoMisura, registryId, id);
            }

            return (null, tipoMisura, registryId, null);
        }
    }
}
