using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace MisureRicci.Services
{
    public class LegacyMeasurementUiService : ILegacyMeasurementUiService
    {
        private const string GiaccaProperty = "Giacca";
        private const string PantaloneProperty = "Pantalone";
        private const string NotesProperty = "Notes";

        public int GetClienteId(object model)
        {
            return (int)(model.GetType().GetProperty("ClienteId")?.GetValue(model)
                ?? throw new InvalidOperationException("ClienteId non disponibile per la misura richiesta."));
        }

        public bool TryApplyEditableMeasurementFields(object model, IEnumerable<LegacyMeasurementFieldViewModel> fields, Action<string, string> addError)
        {
            var valuesByName = fields.ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);
            var isValid = ApplyEditableMeasurementFields(model, valuesByName, addError);

            if (model is AbitoCompletoMeasurement abito)
            {
                abito.Giacca ??= new GiaccaMeasurement { ClienteId = abito.ClienteId };
                abito.Pantalone ??= new PantaloneMeasurement { ClienteId = abito.ClienteId };

                isValid &= ApplyEditableMeasurementFields(abito.Giacca, valuesByName, addError, GiaccaProperty, includeNotes: false);
                isValid &= ApplyEditableMeasurementFields(abito.Pantalone, valuesByName, addError, PantaloneProperty, includeNotes: false);
            }

            return isValid;
        }

        public LegacyMeasurementEditViewModel BuildEditViewModel(object model, string tipoMisura, IEnumerable<LegacyMeasurementFieldViewModel>? postedFields = null)
        {
            var fields = BuildFieldViewModels(model).ToList();
            var postedValues = postedFields?.ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

            if (postedValues != null)
            {
                foreach (var field in fields)
                {
                    if (postedValues.TryGetValue(field.Name, out var postedValue))
                        field.Value = postedValue;
                }
            }

            return new LegacyMeasurementEditViewModel
            {
                Id = GetMeasurementId(model),
                ClienteId = GetClienteId(model),
                TipoMisura = tipoMisura,
                Fields = fields,
                CanEditFields = true,
                WarningMessage = null
            };
        }

        public LegacyMeasurementDetailsViewModel BuildDetailsViewModel(object model, string tipoMisura)
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
                var giacca = measurementType.GetProperty(GiaccaProperty)?.GetValue(model);
                var pantalone = measurementType.GetProperty(PantaloneProperty)?.GetValue(model);

                if (giacca != null)
                {
                    details.Sections.Add(new LegacyMeasurementSectionViewModel
                    {
                        Title = GiaccaProperty,
                        Fields = BuildDisplayFields(giacca).ToList()
                    });
                }

                if (pantalone != null)
                {
                    details.Sections.Add(new LegacyMeasurementSectionViewModel
                    {
                        Title = PantaloneProperty,
                        Fields = BuildDisplayFields(pantalone).ToList()
                    });
                }

                details.Fields.Clear();
            }

            return details;
        }

        public LegacyMeasurementDeleteViewModel BuildDeleteViewModel(object model, string tipoMisura, int? registryId)
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
            if (model is AbitoCompletoMeasurement abito)
            {
                var fields = new List<LegacyMeasurementFieldViewModel>();

                if (abito.Giacca != null)
                {
                    fields.AddRange(BuildEditableFieldViewModels(abito.Giacca, GiaccaProperty, includeNotes: false));
                }

                if (abito.Pantalone != null)
                {
                    fields.AddRange(BuildEditableFieldViewModels(abito.Pantalone, PantaloneProperty, includeNotes: false));
                }

                fields.AddRange(BuildEditableFieldViewModels(abito));
                return fields;
            }

            return BuildEditableFieldViewModels(model);
        }

        private static IEnumerable<LegacyMeasurementFieldViewModel> BuildDisplayFields(object model)
        {
            return model.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    p.Name != "Id" &&
                    p.Name != "ClienteId" &&
                    p.Name != "Cliente" &&
                    p.Name != "CreatedAt" &&
                    p.Name != "OrderId" &&
                    p.Name != "Notes" &&
                    p.Name != GiaccaProperty &&
                    p.Name != PantaloneProperty)
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

        private static IEnumerable<LegacyMeasurementFieldViewModel> BuildEditableFieldViewModels(object model, string? sectionPrefix = null, bool includeNotes = true)
        {
            return GetEditableMeasurementProperties(model.GetType(), includeNotes)
                .Select(property =>
                {
                    var displayName = property.GetCustomAttribute<DisplayAttribute>()?.Name ?? property.Name;
                    if (!string.IsNullOrWhiteSpace(sectionPrefix))
                    {
                        displayName = $"{sectionPrefix} - {displayName}";
                    }

                    return new LegacyMeasurementFieldViewModel
                    {
                        Name = BuildFieldName(property.Name, sectionPrefix),
                        DisplayName = displayName,
                        Value = ConvertToDisplayValue(property.GetValue(model)),
                        IsMultiline = property.Name == NotesProperty
                    };
                });
        }

        private static bool ApplyEditableMeasurementFields(
            object model,
            IReadOnlyDictionary<string, string?> valuesByName,
            Action<string, string> addError,
            string? sectionPrefix = null,
            bool includeNotes = true)
        {
            var isValid = true;

            foreach (var property in GetEditableMeasurementProperties(model.GetType(), includeNotes))
            {
                var fieldName = BuildFieldName(property.Name, sectionPrefix);
                if (!valuesByName.TryGetValue(fieldName, out var rawValue))
                {
                    continue;
                }

                if (!TryConvertFormValue(property.PropertyType, rawValue, out var convertedValue))
                {
                    addError(fieldName, $"Valore non valido per il campo {property.Name}.");
                    isValid = false;
                    continue;
                }

                property.SetValue(model, convertedValue);
            }

            return isValid;
        }

        private static string BuildFieldName(string propertyName, string? sectionPrefix)
        {
            return string.IsNullOrWhiteSpace(sectionPrefix)
                ? propertyName
                : $"{sectionPrefix}.{propertyName}";
        }

        private static IEnumerable<PropertyInfo> GetEditableMeasurementProperties(Type type, bool includeNotes = true)
        {
            return type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .Where(p =>
                    p.Name != "Id" &&
                    p.Name != "ClienteId" &&
                    p.Name != "Cliente" &&
                    p.Name != "CreatedAt" &&
                    p.Name != "OrderId" &&
                    (includeNotes || p.Name != NotesProperty) &&
                    p.Name != GiaccaProperty &&
                    p.Name != PantaloneProperty);
        }

        private static bool TryConvertFormValue(Type propertyType, string? rawValue, out object? convertedValue)
        {
            var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            // 1. String case
            if (targetType == typeof(string))
            {
                convertedValue = rawValue?.Trim();
                return true;
            }

            // 2. Empty value case
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                convertedValue = Nullable.GetUnderlyingType(propertyType) != null
                    ? null
                    : Activator.CreateInstance(targetType);
                return true;
            }

            // 3. Dispatch to type-specific converters
            return TryConvertByType(targetType, rawValue.Trim(), out convertedValue);
        }

        private static bool TryConvertByType(Type targetType, string raw, out object? value)
        {
            if (targetType == typeof(double))
                return TryParseDouble(raw, out value);

            if (targetType == typeof(int))
                return TryParseInt(raw, out value);

            if (targetType == typeof(bool))
                return TryParseBool(raw, out value);

            value = null;
            return false;
        }

        private static bool TryParseDouble(string raw, out object? value)
        {
            // Invariant culture case: dot without comma
            if (raw.Contains('.') &&
                !raw.Contains(',') &&
                double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands,
                                CultureInfo.InvariantCulture, out var inv))
            {
                value = inv;
                return true;
            }

            // Try current culture, then invariant
            if (double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands,
                                CultureInfo.CurrentCulture, out var cur) ||
                double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands,
                                CultureInfo.InvariantCulture, out cur))
            {
                value = cur;
                return true;
            }

            value = null;
            return false;
        }

        private static bool TryParseInt(string raw, out object? value)
        {
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.CurrentCulture, out var result))
            {
                value = result;
                return true;
            }

            value = null;
            return false;
        }

        private static bool TryParseBool(string raw, out object? value)
        {
            if (bool.TryParse(raw, out var result))
            {
                value = result;
                return true;
            }

            value = null;
            return false;
        }

    }
}
