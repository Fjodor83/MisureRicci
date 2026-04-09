using FluentAssertions;
using MisureRicci.Models;
using Xunit;

namespace MisureRicci.Tests.Unit.Helpers;

public class MeasurementUnitHelperTests
{
    [Fact]
    public void TestConvertCmToInch_ReturnsCorrectValue()
    {
        var converted = MeasurementUnitHelper.ConvertStorageToDisplay(
            rawValue: "2.54",
            fieldType: DynamicFieldType.Decimal,
            baseUnit: "cm",
            selectedUnit: MeasurementUnit.Inches);

        converted.Should().Be("1");
    }

    [Fact]
    public void TestConvertInchToCm_ReturnsCorrectValue()
    {
        var converted = MeasurementUnitHelper.ConvertDisplayToStorage(
            rawValue: "1",
            fieldType: DynamicFieldType.Decimal,
            baseUnit: "cm",
            selectedUnit: MeasurementUnit.Inches);

        converted.Should().Be("2.54");
    }

    [Fact]
    public void TestConvertNullValue_ReturnsNull()
    {
        var toStorage = MeasurementUnitHelper.ConvertDisplayToStorage(
            rawValue: null,
            fieldType: DynamicFieldType.Decimal,
            baseUnit: "cm",
            selectedUnit: MeasurementUnit.Centimeters);

        var toDisplay = MeasurementUnitHelper.ConvertStorageToDisplay(
            rawValue: null,
            fieldType: DynamicFieldType.Decimal,
            baseUnit: "cm",
            selectedUnit: MeasurementUnit.Centimeters);

        toStorage.Should().BeNull();
        toDisplay.Should().BeNull();
    }

    [Fact]
    public void TestNonConvertibleField_ReturnsOriginalValue()
    {
        var raw = "M";

        var toStorage = MeasurementUnitHelper.ConvertDisplayToStorage(
            rawValue: raw,
            fieldType: DynamicFieldType.Text,
            baseUnit: "cm",
            selectedUnit: MeasurementUnit.Inches);

        var toDisplay = MeasurementUnitHelper.ConvertStorageToDisplay(
            rawValue: raw,
            fieldType: DynamicFieldType.Text,
            baseUnit: "cm",
            selectedUnit: MeasurementUnit.Inches);

        toStorage.Should().Be(raw);
        toDisplay.Should().Be(raw);
    }
}
