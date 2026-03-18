using MisureRicci.Models;

namespace MisureRicci.Services
{
    public static class MeasurementTypeSeedData
    {
        public static IReadOnlyList<MeasurementType> GetDefaultTypes()
        {
            return new List<MeasurementType>
            {
                Build("Giacca", "Misure specifiche per giacche e blazer.", new[]
                {
                    Field("spalle", "Spalle", DynamicFieldType.Decimal, "cm", true, 1, "Struttura", 1, DynamicFieldTemplate.Emphasis, "Misura da cucitura a cucitura"),
                    Field("torace", "Torace", DynamicFieldType.Decimal, "cm", true, 2, "Volumi", 2, DynamicFieldTemplate.Emphasis, "Circonferenza piena del torace"),
                    Field("vita", "Vita", DynamicFieldType.Decimal, "cm", true, 3, "Volumi", 2, DynamicFieldTemplate.Standard, null),
                    Field("manica", "Manica", DynamicFieldType.Decimal, "cm", true, 4, "Lunghezze", 3, DynamicFieldTemplate.Compact, null),
                    Field("lunghezza", "Lunghezza", DynamicFieldType.Decimal, "cm", true, 5, "Lunghezze", 3, DynamicFieldTemplate.Compact, null)
                }),
                Build("Pantalone", "Misure per pantaloni sartoriali e casual.", new[]
                {
                    Field("vita", "Vita", DynamicFieldType.Decimal, "cm", true, 1, "Assetto", 1, DynamicFieldTemplate.Emphasis, null),
                    Field("bacino", "Bacino", DynamicFieldType.Decimal, "cm", true, 2, "Assetto", 1, DynamicFieldTemplate.Standard, null),
                    Field("cavallo", "Cavallo", DynamicFieldType.Decimal, "cm", true, 3, "Gamba", 2, DynamicFieldTemplate.Compact, null),
                    Field("interno_gamba", "Interno Gamba", DynamicFieldType.Decimal, "cm", true, 4, "Gamba", 2, DynamicFieldTemplate.Compact, null),
                    Field("fondo", "Fondo", DynamicFieldType.Decimal, "cm", true, 5, "Finale", 3, DynamicFieldTemplate.Compact, null)
                }),
                Build("Abito", "Configurazione completa giacca e pantalone.", new[]
                {
                    Field("giacca_spalle", "Giacca Spalle", DynamicFieldType.Decimal, "cm", true, 1, "Giacca", 1, DynamicFieldTemplate.Emphasis, null),
                    Field("giacca_torace", "Giacca Torace", DynamicFieldType.Decimal, "cm", true, 2, "Giacca", 1, DynamicFieldTemplate.Standard, null),
                    Field("giacca_vita", "Giacca Vita", DynamicFieldType.Decimal, "cm", true, 3, "Giacca", 1, DynamicFieldTemplate.Standard, null),
                    Field("giacca_manica", "Giacca Manica", DynamicFieldType.Decimal, "cm", true, 4, "Giacca", 1, DynamicFieldTemplate.Compact, null),
                    Field("giacca_lunghezza", "Giacca Lunghezza", DynamicFieldType.Decimal, "cm", true, 5, "Giacca", 1, DynamicFieldTemplate.Compact, null),
                    Field("pantalone_vita", "Pantalone Vita", DynamicFieldType.Decimal, "cm", true, 6, "Pantalone", 2, DynamicFieldTemplate.Emphasis, null),
                    Field("pantalone_bacino", "Pantalone Bacino", DynamicFieldType.Decimal, "cm", true, 7, "Pantalone", 2, DynamicFieldTemplate.Standard, null),
                    Field("pantalone_cavallo", "Pantalone Cavallo", DynamicFieldType.Decimal, "cm", true, 8, "Pantalone", 2, DynamicFieldTemplate.Compact, null),
                    Field("pantalone_interno_gamba", "Pantalone Interno Gamba", DynamicFieldType.Decimal, "cm", true, 9, "Pantalone", 2, DynamicFieldTemplate.Compact, null),
                    Field("pantalone_fondo", "Pantalone Fondo", DynamicFieldType.Decimal, "cm", true, 10, "Pantalone", 2, DynamicFieldTemplate.Compact, null)
                }),
                Build("Gilet", "Misure per gilet eleganti e sportivi.", new[]
                {
                    Field("torace", "Torace", DynamicFieldType.Decimal, "cm", true, 1, "Corpo", 1, DynamicFieldTemplate.Emphasis, null),
                    Field("vita", "Vita", DynamicFieldType.Decimal, "cm", true, 2, "Corpo", 1, DynamicFieldTemplate.Standard, null),
                    Field("lunghezza", "Lunghezza", DynamicFieldType.Decimal, "cm", true, 3, "Lunghezze", 2, DynamicFieldTemplate.Compact, null)
                }),
                Build("Maglie", "Misure per pullover, cardigan e maglieria.", new[]
                {
                    Field("torace", "Torace", DynamicFieldType.Decimal, "cm", true, 1, "Corpo", 1, DynamicFieldTemplate.Emphasis, null),
                    Field("spalle", "Spalle", DynamicFieldType.Decimal, "cm", true, 2, "Corpo", 1, DynamicFieldTemplate.Standard, null),
                    Field("manica", "Manica", DynamicFieldType.Decimal, "cm", true, 3, "Lunghezze", 2, DynamicFieldTemplate.Compact, null),
                    Field("lunghezza", "Lunghezza", DynamicFieldType.Decimal, "cm", true, 4, "Lunghezze", 2, DynamicFieldTemplate.Compact, null)
                }),
                Build("Outdoor", "Abbigliamento tecnico, cappotti e giacconi.", new[]
                {
                    Field("torace", "Torace", DynamicFieldType.Decimal, "cm", true, 1, "Corpo", 1, DynamicFieldTemplate.Emphasis, null),
                    Field("spalle", "Spalle", DynamicFieldType.Decimal, "cm", true, 2, "Corpo", 1, DynamicFieldTemplate.Standard, null),
                    Field("manica", "Manica", DynamicFieldType.Decimal, "cm", true, 3, "Lunghezze", 2, DynamicFieldTemplate.Compact, null),
                    Field("lunghezza", "Lunghezza", DynamicFieldType.Decimal, "cm", true, 4, "Lunghezze", 2, DynamicFieldTemplate.Compact, null),
                    Field("fit", "Vestibilità", DynamicFieldType.Text, null, false, 5, "Note sartoriali", 3, DynamicFieldTemplate.Notes, "Slim, regular, comfort")
                }),
                Build("Camicia", "Misure per camicie sartoriali.", new[]
                {
                    Field("intro_struttura", "Imposta prima le misure strutturali", DynamicFieldType.Text, null, false, 0, "Struttura", 1, DynamicFieldTemplate.SectionHeader, "Compila i campi base prima delle finiture"),
                    Field("collo", "Collo", DynamicFieldType.Decimal, "cm", true, 1, "Struttura", 1, DynamicFieldTemplate.Emphasis, "Misura collo finito"),
                    Field("spalle", "Spalle", DynamicFieldType.Decimal, "cm", true, 2, "Struttura", 1, DynamicFieldTemplate.Standard, null),
                    Field("sep_corpo", "", DynamicFieldType.Text, null, false, 3, "Corpo", 2, DynamicFieldTemplate.Divider, null),
                    Field("torace", "Torace", DynamicFieldType.Decimal, "cm", true, 3, "Corpo", 2, DynamicFieldTemplate.Emphasis, null),
                    Field("vita", "Vita", DynamicFieldType.Decimal, "cm", true, 4, "Corpo", 2, DynamicFieldTemplate.Standard, null),
                    Field("manica", "Manica", DynamicFieldType.Decimal, "cm", true, 5, "Maniche", 3, DynamicFieldTemplate.TwoColumnLocked, null),
                    Field("polso", "Polso", DynamicFieldType.Decimal, "cm", true, 6, "Maniche", 3, DynamicFieldTemplate.TwoColumnLocked, null),
                    Field("lunghezza", "Lunghezza", DynamicFieldType.Decimal, "cm", true, 7, "Lunghezze", 4, DynamicFieldTemplate.Compact, null),
                    Field("alert_note_fit", "Attenzione vestibilità", DynamicFieldType.Text, null, false, 8, "Note sartoriali", 5, DynamicFieldTemplate.AlertNote, "Se il cliente richiede fit slim, verifica margini su torace e vita")
                }),
                Build("Scarpe", "Taglia e misure del piede.", new[]
                {
                    Field("taglia", "Taglia", DynamicFieldType.Text, null, true, 1, "Base", 1, DynamicFieldTemplate.Emphasis, null),
                    Field("lunghezza_piede", "Lunghezza Piede", DynamicFieldType.Decimal, "cm", true, 2, "Base", 1, DynamicFieldTemplate.Standard, null),
                    Field("pianta", "Larghezza Pianta", DynamicFieldType.Text, null, false, 3, "Base", 1, DynamicFieldTemplate.Compact, null)
                }),
                Build("Cravatta", "Misure per cravatte su misura.", new[]
                {
                    Field("lunghezza", "Lunghezza", DynamicFieldType.Decimal, "cm", true, 1, "Base", 1, DynamicFieldTemplate.Emphasis, null),
                    Field("larghezza", "Larghezza", DynamicFieldType.Decimal, "cm", true, 2, "Base", 1, DynamicFieldTemplate.Standard, null)
                }),
                Build("Cintura", "Lunghezza e girovita per cinture.", new[]
                {
                    Field("lunghezza", "Lunghezza Totale", DynamicFieldType.Decimal, "cm", true, 1, "Base", 1, DynamicFieldTemplate.Emphasis, null),
                    Field("girovita", "Girovita", DynamicFieldType.Decimal, "cm", true, 2, "Base", 1, DynamicFieldTemplate.Standard, null)
                })
            };
        }

        private static MeasurementType Build(string nome, string descrizione, IEnumerable<MeasurementFieldDefinition> campi)
        {
            return new MeasurementType
            {
                Nome = nome,
                Descrizione = descrizione,
                IsActive = true,
                IsSystem = true,
                Campi = campi.ToList()
            };
        }

        private static MeasurementFieldDefinition Field(string nome, string etichetta, DynamicFieldType tipo, string? unita, bool obbligatorio, int ordine, string? gruppo, int ordineGruppo, DynamicFieldTemplate template, string? helpText)
        {
            return new MeasurementFieldDefinition
            {
                NomeCampo = nome,
                Etichetta = etichetta,
                Gruppo = gruppo,
                OrdineGruppo = ordineGruppo,
                TipoDato = tipo,
                Template = template,
                UnitaMisura = unita,
                Obbligatorio = obbligatorio,
                Ordine = ordine,
                IsActive = true,
                Placeholder = etichetta,
                HelpText = helpText
            };
        }
    }
}
