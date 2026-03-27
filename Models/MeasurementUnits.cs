using System.Globalization;

namespace MisureRicci.Models
{
    public enum MeasurementUnit
    {
        Centimeters = 0,
        Inches = 1
    }

    public static class MeasurementUnitHelper
    {
        private const decimal CentimetersPerInch = 2.54m;

        public static string ToShortLabel(MeasurementUnit unit)
        {
            return unit == MeasurementUnit.Inches ? "INCH" : "CM";
        }

        public static string ToDisplayUnit(MeasurementUnit unit)
        {
            return unit == MeasurementUnit.Inches ? "inch" : "cm";
        }

        public static bool SupportsConversion(DynamicFieldType fieldType, string? baseUnit)
        {
            if (fieldType != DynamicFieldType.Decimal && fieldType != DynamicFieldType.Number)
            {
                return false;
            }

            return IsCentimeterUnit(baseUnit);
        }

        public static string? GetFieldUnitLabel(string? baseUnit, DynamicFieldType fieldType, MeasurementUnit selectedUnit)
        {
            return SupportsConversion(fieldType, baseUnit)
                ? ToDisplayUnit(selectedUnit)
                : baseUnit;
        }

        public static string? ConvertDisplayToStorage(
            string? rawValue,
            DynamicFieldType fieldType,
            string? baseUnit,
            MeasurementUnit selectedUnit)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return rawValue?.Trim();
            }

            var trimmedValue = rawValue.Trim();
            if (!SupportsConversion(fieldType, baseUnit))
            {
                return trimmedValue;
            }

            if (!TryParseDecimal(trimmedValue, out var numericValue))
            {
                return trimmedValue;
            }

            var centimetersValue = selectedUnit == MeasurementUnit.Inches
                ? numericValue * CentimetersPerInch
                : numericValue;

            return FormatDecimal(centimetersValue);
        }

        public static string? ConvertStorageToDisplay(
            string? rawValue,
            DynamicFieldType fieldType,
            string? baseUnit,
            MeasurementUnit selectedUnit)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return rawValue?.Trim();
            }

            var trimmedValue = rawValue.Trim();
            if (!SupportsConversion(fieldType, baseUnit))
            {
                return trimmedValue;
            }

            if (!TryParseDecimal(trimmedValue, out var numericValue))
            {
                return trimmedValue;
            }

            var displayValue = selectedUnit == MeasurementUnit.Inches
                ? numericValue / CentimetersPerInch
                : numericValue;

            return FormatDecimal(displayValue);
        }

        public static bool TryParseDecimal(string? rawValue, out decimal value)
        {
            var normalizedValue = rawValue?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedValue))
            {
                value = 0;
                return false;
            }

            normalizedValue = normalizedValue.Replace(" ", string.Empty);

            if (normalizedValue.Contains('.') && normalizedValue.Contains(','))
            {
                var lastDot = normalizedValue.LastIndexOf('.');
                var lastComma = normalizedValue.LastIndexOf(',');
                normalizedValue = lastDot > lastComma
                    ? normalizedValue.Replace(",", string.Empty)
                    : normalizedValue.Replace(".", string.Empty).Replace(',', '.');
            }
            else if (normalizedValue.Contains(','))
            {
                normalizedValue = normalizedValue.Replace(',', '.');
            }

            return decimal.TryParse(
                       normalizedValue,
                       NumberStyles.Float,
                       CultureInfo.InvariantCulture,
                       out value)
                   || decimal.TryParse(
                       normalizedValue,
                       NumberStyles.Float,
                       CultureInfo.CurrentCulture,
                       out value);
        }

        public static string FormatDecimal(decimal value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static bool IsCentimeterUnit(string? unit)
        {
            if (string.IsNullOrWhiteSpace(unit))
            {
                return false;
            }

            return unit.Trim().ToLowerInvariant() is "cm" or "centimetri" or "centimeters" or "centimetres";
        }
    }
}
