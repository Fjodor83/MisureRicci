using MisureRicci.Models;

namespace MisureRicci.Services
{
    internal static class CommessaAccessHelper
    {
        internal static readonly Dictionary<StatoCommessa, StatoCommessa[]> AllowedTransitions = new()
        {
            [StatoCommessa.Bozza] = new[] { StatoCommessa.MisureRaccolte, StatoCommessa.Annullata },
            [StatoCommessa.MisureRaccolte] = new[] { StatoCommessa.InLavorazione, StatoCommessa.Annullata },
            [StatoCommessa.InLavorazione] = new[] { StatoCommessa.Prova1, StatoCommessa.ProntaConsegna, StatoCommessa.Annullata },
            [StatoCommessa.Prova1] = new[] { StatoCommessa.Prova2, StatoCommessa.InLavorazione, StatoCommessa.Annullata },
            [StatoCommessa.Prova2] = new[] { StatoCommessa.InLavorazione, StatoCommessa.ProntaConsegna, StatoCommessa.Annullata },
            [StatoCommessa.ProntaConsegna] = new[] { StatoCommessa.Consegnata, StatoCommessa.InLavorazione, StatoCommessa.Annullata },
            [StatoCommessa.Consegnata] = Array.Empty<StatoCommessa>(),
            [StatoCommessa.Annullata] = Array.Empty<StatoCommessa>()
        };

        internal static readonly HashSet<StatoCommessa> StatiConMisuraObbligatoria = new()
        {
            StatoCommessa.MisureRaccolte,
            StatoCommessa.InLavorazione,
            StatoCommessa.Prova1,
            StatoCommessa.Prova2,
            StatoCommessa.ProntaConsegna,
            StatoCommessa.Consegnata
        };

        internal static bool CanAccessNegozio(int? commessaNegozioId, int? userNegozioId, bool isAdmin)
        {
            if (isAdmin) return true;
            return userNegozioId.HasValue
                && commessaNegozioId.HasValue
                && commessaNegozioId.Value == userNegozioId.Value;
        }

        internal static List<StatoCommessa> GetAllowedNextStates(StatoCommessa current)
        {
            if (!AllowedTransitions.TryGetValue(current, out var items))
                return new List<StatoCommessa>();
            return items.ToList();
        }

        internal static bool RichiedeMisuraCollegata(StatoCommessa stato) =>
            StatiConMisuraObbligatoria.Contains(stato);

        internal static bool IsMeasurementRecommendedForTipoCapo(string tipoCapo, string tipoMisura)
        {
            if (string.IsNullOrWhiteSpace(tipoCapo) || string.IsNullOrWhiteSpace(tipoMisura))
                return false;

            var firstTipoCapoWord = tipoCapo
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            return tipoCapo.Contains(tipoMisura, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(firstTipoCapoWord)
                    && tipoMisura.Contains(firstTipoCapoWord, StringComparison.OrdinalIgnoreCase));
        }

        internal static async Task<(T? Value, string? Error)> FetchAndAuthorizeAsync<T>(
            IQueryable<T> query,
            Func<T, int?> negozioIdSelector,
            int? userNegozioId,
            bool isAdmin,
            string notFoundMessage = "Risorsa non trovata.",
            string forbidMessage = "Accesso negato.")
            where T : class
        {
            var entity = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(query);
            if (entity is null)
                return (null, notFoundMessage);
            if (!CanAccessNegozio(negozioIdSelector(entity), userNegozioId, isAdmin))
                return (null, forbidMessage);
            return (entity, null);
        }
    }
}
