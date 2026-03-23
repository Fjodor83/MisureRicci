namespace MisureRicci.Models
{
    public enum TipoMisuraLegacy
    {
        Giacca,
        Pantalone,
        Abito,
        Gilet,
        Maglie,
        Outdoor,
        Camicia,
        Scarpe,
        Cravatta,
        Cintura
    }

    public static class TipoMisuraLegacyExtensions
    {
        public static string ToTableName(this TipoMisuraLegacy tipo) => tipo switch
        {
            TipoMisuraLegacy.Giacca => "MisureGiacca",
            TipoMisuraLegacy.Pantalone => "MisurePantalone",
            TipoMisuraLegacy.Abito => "MisureAbitoCompleto",
            TipoMisuraLegacy.Gilet => "MisureGilet",
            TipoMisuraLegacy.Maglie => "MisureMaglie",
            TipoMisuraLegacy.Outdoor => "MisureOutdoor",
            TipoMisuraLegacy.Camicia => "MisureCamicia",
            TipoMisuraLegacy.Scarpe => "MisureScarpe",
            TipoMisuraLegacy.Cravatta => "MisureCravatta",
            TipoMisuraLegacy.Cintura => "MisureCintura",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
        };

        public static string ToDomainName(this TipoMisuraLegacy tipo) => tipo switch
        {
            TipoMisuraLegacy.Giacca => "giacca",
            TipoMisuraLegacy.Pantalone => "pantalone",
            TipoMisuraLegacy.Abito => "abito",
            TipoMisuraLegacy.Gilet => "gilet",
            TipoMisuraLegacy.Maglie => "maglie",
            TipoMisuraLegacy.Outdoor => "outdoor",
            TipoMisuraLegacy.Camicia => "camicia",
            TipoMisuraLegacy.Scarpe => "scarpe",
            TipoMisuraLegacy.Cravatta => "cravatta",
            TipoMisuraLegacy.Cintura => "cintura",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
        };
    }
}
