using MisureRicci.Models;

namespace MisureRicci.Services
{
    /// <summary>
    /// Converts a static legacy measurement entity into a <see cref="DynamicMeasurementRecord"/>
    /// backed by the corresponding <see cref="MeasurementType"/> seed data.
    /// This is the first step in migrating historical data away from the 10 fixed tables
    /// toward the single generic dynamic model.
    /// </summary>
    public interface ILegacyMeasurementConverter
    {
        /// <summary>
        /// Converts <paramref name="legacy"/> to a <see cref="DynamicMeasurementRecord"/> and
        /// adds a new <see cref="MisureCliente"/> registry entry (IsDynamic = true).
        /// The original legacy record and its registry entry are NOT deleted — the caller
        /// decides whether to remove them after verifying the conversion.
        /// </summary>
        /// <returns>
        /// The newly created <see cref="DynamicMeasurementRecord"/>, or <c>null</c> if no
        /// active <see cref="MeasurementType"/> with a matching name was found.
        /// </returns>
        Task<DynamicMeasurementRecord?> ConvertAsync(
            BaseMeasurement legacy,
            string tipoMisura,
            string createdByUserId,
            CancellationToken cancellationToken = default);
    }
}
