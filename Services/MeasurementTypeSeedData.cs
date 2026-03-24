using MisureRicci.Models;

namespace MisureRicci.Services
{
    public static class MeasurementTypeSeedData
    {
        private const string Giacca = "Giacca";
        private const string Pantalone = "Pantalone";
        private const string Struttura = "Struttura";
        private const string ToraceLower = "torace";
        private const string ToraceUpper = "Torace";
        private const string Lunghezze = "Lunghezze";
        private const string LunghezzaLower = "lunghezza";
        private const string LunghezzaUpper = "Lunghezza";
        private const string Corpo = "Corpo";
        private const string UnitCm = "cm";

        public static IReadOnlyList<MeasurementType> GetDefaultTypes()
        {
            return new List<MeasurementType>
            {
                Build(Giacca, "Misure specifiche per giacche e blazer.", new[]
                {
                    Field("spalle", "Spalle", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 1, Gruppo = Struttura, OrdineGruppo = 1, Template = DynamicFieldTemplate.Emphasis, HelpText = "Misura da cucitura a cucitura" }),
                    Field(ToraceLower, ToraceUpper, new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 2, Gruppo = "Volumi", OrdineGruppo = 2, Template = DynamicFieldTemplate.Emphasis, HelpText = "Circonferenza piena del torace" }),
                    Field("vita", "Vita", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 3, Gruppo = "Volumi", OrdineGruppo = 2, Template = DynamicFieldTemplate.Standard }),
                    Field("manica", "Manica", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 4, Gruppo = Lunghezze, OrdineGruppo = 3, Template = DynamicFieldTemplate.Compact }),
                    Field(LunghezzaLower, LunghezzaUpper, new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 5, Gruppo = Lunghezze, OrdineGruppo = 3, Template = DynamicFieldTemplate.Compact })
                }),
                Build(Pantalone, "Misure per pantaloni sartoriali e casual.", new[]
                {
                    Field("vita", "Vita", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 1, Gruppo = "Assetto", OrdineGruppo = 1, Template = DynamicFieldTemplate.Emphasis }),
                    Field("bacino", "Bacino", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 2, Gruppo = "Assetto", OrdineGruppo = 1, Template = DynamicFieldTemplate.Standard }),
                    Field("cavallo", "Cavallo", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 3, Gruppo = "Gamba", OrdineGruppo = 2, Template = DynamicFieldTemplate.Compact }),
                    Field("interno_gamba", "Interno Gamba", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 4, Gruppo = "Gamba", OrdineGruppo = 2, Template = DynamicFieldTemplate.Compact }),
                    Field("fondo", "Fondo", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 5, Gruppo = "Finale", OrdineGruppo = 3, Template = DynamicFieldTemplate.Compact })
                }),
                Build("Abito", "Configurazione completa giacca e pantalone.", new[]
                {
                    Field("giacca_spalle", "Giacca Spalle", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 1, Gruppo = Giacca, OrdineGruppo = 1, Template = DynamicFieldTemplate.Emphasis }),
                    Field("giacca_torace", "Giacca Torace", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 2, Gruppo = Giacca, OrdineGruppo = 1, Template = DynamicFieldTemplate.Standard }),
                    Field("giacca_vita", "Giacca Vita", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 3, Gruppo = Giacca, OrdineGruppo = 1, Template = DynamicFieldTemplate.Standard }),
                    Field("giacca_manica", "Giacca Manica", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 4, Gruppo = Giacca, OrdineGruppo = 1, Template = DynamicFieldTemplate.Compact }),
                    Field("giacca_lunghezza", "Giacca Lunghezza", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 5, Gruppo = Giacca, OrdineGruppo = 1, Template = DynamicFieldTemplate.Compact }),
                    Field("pantalone_vita", "Pantalone Vita", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 6, Gruppo = Pantalone, OrdineGruppo = 2, Template = DynamicFieldTemplate.Emphasis }),
                    Field("pantalone_bacino", "Pantalone Bacino", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 7, Gruppo = Pantalone, OrdineGruppo = 2, Template = DynamicFieldTemplate.Standard }),
                    Field("pantalone_cavallo", "Pantalone Cavallo", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 8, Gruppo = Pantalone, OrdineGruppo = 2, Template = DynamicFieldTemplate.Compact }),
                    Field("pantalone_interno_gamba", "Pantalone Interno Gamba", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 9, Gruppo = Pantalone, OrdineGruppo = 2, Template = DynamicFieldTemplate.Compact }),
                    Field("pantalone_fondo", "Pantalone Fondo", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 10, Gruppo = Pantalone, OrdineGruppo = 2, Template = DynamicFieldTemplate.Compact })
                }),
                Build("Gilet", "Misure per gilet eleganti e sportivi.", new[]
                {
                    Field(ToraceLower, ToraceUpper, new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 1, Gruppo = Corpo, OrdineGruppo = 1, Template = DynamicFieldTemplate.Emphasis }),
                    Field("vita", "Vita", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 2, Gruppo = Corpo, OrdineGruppo = 1, Template = DynamicFieldTemplate.Standard }),
                    Field(LunghezzaLower, LunghezzaUpper, new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 3, Gruppo = Lunghezze, OrdineGruppo = 2, Template = DynamicFieldTemplate.Compact })
                }),
                Build("Maglie", "Misure per pullover, cardigan e maglieria.", new[]
                {
                    Field("Width / Larghezza", "A-1", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 1, Gruppo = Lunghezze, OrdineGruppo = 2, Template = DynamicFieldTemplate.Emphasis }),
                    Field("Length / Lunghezza", "B-2", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 2, Gruppo = Lunghezze, OrdineGruppo = 2, Template = DynamicFieldTemplate.Compact }),
                    Field("Waist / Vita", "C-3", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 3, Gruppo = Corpo, OrdineGruppo = 1, Template = DynamicFieldTemplate.Emphasis }),
                    Field("Shoulder / Spalle", "D-4", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 4, Gruppo = Corpo, OrdineGruppo = 1, Template = DynamicFieldTemplate.Emphasis }),
                    Field("Shoulder Length / Lungh. Manica", "E-5", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 5, Gruppo = Lunghezze, OrdineGruppo = 2, Template = DynamicFieldTemplate.Compact })
                }),

                Build("Outdoor", "Abbigliamento tecnico, cappotti e giacconi.", new[]
                {
                    Field(ToraceLower, ToraceUpper, new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 1, Gruppo = Corpo, OrdineGruppo = 1, Template = DynamicFieldTemplate.Emphasis }),
                    Field("spalle", "Spalle", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 2, Gruppo = Corpo, OrdineGruppo = 1, Template = DynamicFieldTemplate.Standard }),
                    Field("manica", "Manica", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 3, Gruppo = Lunghezze, OrdineGruppo = 2, Template = DynamicFieldTemplate.Compact }),
                    Field(LunghezzaLower, LunghezzaUpper, new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 4, Gruppo = Lunghezze, OrdineGruppo = 2, Template = DynamicFieldTemplate.Compact }),
                    Field("fit", "Vestibilità", new FieldOptions { Tipo = DynamicFieldType.Text, Unita = null, Obbligatorio = false, Ordine = 5, Gruppo = "Note sartoriali", OrdineGruppo = 3, Template = DynamicFieldTemplate.Notes, HelpText = "Slim, regular, comfort" })
                }),
                Build("Camicia", "Misure per camicie sartoriali.", new[]
                {
                    Field("intro_struttura", "Imposta prima le misure strutturali", new FieldOptions { Tipo = DynamicFieldType.Text, Unita = null, Obbligatorio = false, Ordine = 0, Gruppo = Struttura, OrdineGruppo = 1, Template = DynamicFieldTemplate.SectionHeader, HelpText = "Compila i campi base prima delle finiture" }),
                    Field("collo", "Collo", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 1, Gruppo = Struttura, OrdineGruppo = 1, Template = DynamicFieldTemplate.Emphasis, HelpText = "Misura collo finito" }),
                    Field("spalle", "Spalle", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 2, Gruppo = Struttura, OrdineGruppo = 1, Template = DynamicFieldTemplate.Standard }),
                    Field("sep_corpo", "", new FieldOptions { Tipo = DynamicFieldType.Text, Unita = null, Obbligatorio = false, Ordine = 3, Gruppo = Corpo, OrdineGruppo = 2, Template = DynamicFieldTemplate.Divider }),
                    Field(ToraceLower, ToraceUpper, new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 3, Gruppo = Corpo, OrdineGruppo = 2, Template = DynamicFieldTemplate.Emphasis }),
                    Field("vita", "Vita", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 4, Gruppo = Corpo, OrdineGruppo = 2, Template = DynamicFieldTemplate.Standard }),
                    Field("manica", "Manica", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 5, Gruppo = "Maniche", OrdineGruppo = 3, Template = DynamicFieldTemplate.TwoColumnLocked }),
                    Field("polso", "Polso", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 6, Gruppo = "Maniche", OrdineGruppo = 3, Template = DynamicFieldTemplate.TwoColumnLocked }),
                    Field(LunghezzaLower, LunghezzaUpper, new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 7, Gruppo = Lunghezze, OrdineGruppo = 4, Template = DynamicFieldTemplate.Compact }),
                    Field("alert_note_fit", "Attenzione vestibilità", new FieldOptions { Tipo = DynamicFieldType.Text, Unita = null, Obbligatorio = false, Ordine = 8, Gruppo = "Note sartoriali", OrdineGruppo = 5, Template = DynamicFieldTemplate.AlertNote, HelpText = "Se il cliente richiede fit slim, verifica margini su torace e vita" })
                }),
                Build("Scarpe", "Taglia e misure del piede.", new[]
                {
                    Field("taglia", "Taglia", new FieldOptions { Tipo = DynamicFieldType.Text, Unita = null, Obbligatorio = true, Ordine = 1, Gruppo = "Base", OrdineGruppo = 1, Template = DynamicFieldTemplate.Emphasis }),
                    Field("lunghezza_piede", "Lunghezza Piede", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 2, Gruppo = "Base", OrdineGruppo = 1, Template = DynamicFieldTemplate.Standard }),
                    Field("pianta", "Larghezza Pianta", new FieldOptions { Tipo = DynamicFieldType.Text, Unita = null, Obbligatorio = false, Ordine = 3, Gruppo = "Base", OrdineGruppo = 1, Template = DynamicFieldTemplate.Compact })
                }),
                Build("Cravatta", "Misure per cravatte su misura.", new[]
                {
                    Field(LunghezzaLower, LunghezzaUpper, new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 1, Gruppo = "Base", OrdineGruppo = 1, Template = DynamicFieldTemplate.Emphasis }),
                    Field("larghezza", "Larghezza", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 2, Gruppo = "Base", OrdineGruppo = 1, Template = DynamicFieldTemplate.Standard })
                }),
                Build("Cintura", "Lunghezza e girovita per cinture.", new[]
                {
                    Field(LunghezzaLower, "Lunghezza Totale", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 1, Gruppo = "Base", OrdineGruppo = 1, Template = DynamicFieldTemplate.Emphasis }),
                    Field("girovita", "Girovita", new FieldOptions { Tipo = DynamicFieldType.Decimal, Unita = UnitCm, Obbligatorio = true, Ordine = 2, Gruppo = "Base", OrdineGruppo = 1, Template = DynamicFieldTemplate.Standard })
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

        private static MeasurementFieldDefinition Field(string nome, string etichetta, FieldOptions options)
        {
            return new MeasurementFieldDefinition
            {
                NomeCampo = nome,
                Etichetta = etichetta,
                Gruppo = options.Gruppo,
                OrdineGruppo = options.OrdineGruppo,
                TipoDato = options.Tipo,
                Template = options.Template,
                UnitaMisura = options.Unita,
                Obbligatorio = options.Obbligatorio,
                Ordine = options.Ordine,
                IsActive = true,
                Placeholder = etichetta,
                HelpText = options.HelpText
            };
        }

        private struct FieldOptions
        {
            public DynamicFieldType Tipo { get; init; }
            public string? Unita { get; init; }
            public bool Obbligatorio { get; init; }
            public int Ordine { get; init; }
            public string? Gruppo { get; init; }
            public int OrdineGruppo { get; init; }
            public DynamicFieldTemplate Template { get; init; }
            public string? HelpText { get; init; }
        }
    }
}
