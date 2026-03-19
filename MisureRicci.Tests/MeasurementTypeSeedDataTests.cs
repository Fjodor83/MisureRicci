using MisureRicci.Services;
using Xunit;

namespace MisureRicci.Tests;

public class MeasurementTypeSeedDataTests
{
    [Fact]
    public void GetDefaultTypes_ReturnsRequiredCoreTypes()
    {
        var types = MeasurementTypeSeedData.GetDefaultTypes();

        Assert.NotEmpty(types);
        Assert.Contains(types, t => string.Equals(t.Nome, "Giacca", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(types, t => string.Equals(t.Nome, "Camicia", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(types, t => string.Equals(t.Nome, "Pantalone", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetDefaultTypes_ContainsAtLeastOneRequiredFieldPerType()
    {
        var types = MeasurementTypeSeedData.GetDefaultTypes();

        Assert.All(types, t =>
        {
            Assert.NotEmpty(t.Campi);
            Assert.Contains(t.Campi, c => c.Obbligatorio);
        });
    }
}
