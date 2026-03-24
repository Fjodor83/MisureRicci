using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;

namespace MisureRicci.Services
{
    public static class MeasurementTypeSeeder
    {
        public static async Task SeedDefaultsAsync(ApplicationDbContext context)
        {
            var defaults = MeasurementTypeSeedData.GetDefaultTypes();

            foreach (var defaultType in defaults)
            {
                var existingType = await context.DynamicMeasurementTypes
                    .Include(x => x.Campi)
                    .FirstOrDefaultAsync(x => x.Nome == defaultType.Nome);

                if (existingType == null)
                {
                    context.DynamicMeasurementTypes.Add(defaultType);
                    continue;
                }

                existingType.Descrizione = string.IsNullOrWhiteSpace(existingType.Descrizione)
                    ? defaultType.Descrizione
                    : existingType.Descrizione;
                existingType.IsSystem = true;
                existingType.IsActive = true;

                foreach (var defaultField in defaultType.Campi)
                {
                    if (existingType.Campi.Any(x => x.NomeCampo == defaultField.NomeCampo))
                    {
                        continue;
                    }

                    existingType.Campi.Add(new Models.MeasurementFieldDefinition
                    {
                        NomeCampo = defaultField.NomeCampo,
                        Etichetta = defaultField.Etichetta,
                        Gruppo = defaultField.Gruppo,
                        OrdineGruppo = defaultField.OrdineGruppo,
                        TipoDato = defaultField.TipoDato,
                        Template = defaultField.Template,
                        UnitaMisura = defaultField.UnitaMisura,
                        Placeholder = defaultField.Placeholder,
                        HelpText = defaultField.HelpText,
                        Obbligatorio = defaultField.Obbligatorio,
                        Ordine = defaultField.Ordine,
                        IsActive = true
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
