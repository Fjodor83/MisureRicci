using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public class LegacyMeasurementUiService : ILegacyMeasurementUiService
    {
        public int GetClienteId(object model)
        {
            return (int)(model.GetType().GetProperty("ClienteId")?.GetValue(model)
                ?? throw new InvalidOperationException("ClienteId non disponibile per la misura richiesta."));
        }

        public bool TryApplyEditableMeasurementFields(object model, IEnumerable<LegacyMeasurementFieldViewModel> fields, Action<string, string> addError)
        {
            var valuesByName = fields.ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);
            var isValid = true;

            foreach (var property in GetEditableMeasurementProperties(model.GetType()))
            {
                if (!valuesByName.TryGetValue(property.Name, out var rawValue))
                {
                    continue;
                }

                if (!TryConvertFormValue(property.PropertyType, rawValue, out var convertedValue))
                {
                    addError(property.Name, $"Valore non valido per il campo {property.Name}.");
                    isValid = false;
                    continue;
                }

                property.SetValue(model, convertedValue);
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
    }
}
