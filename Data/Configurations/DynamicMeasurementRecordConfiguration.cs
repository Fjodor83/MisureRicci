using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MisureRicci.Models;

namespace MisureRicci.Data.Configurations
{
    /// <summary>
    /// Configurazione EF Core per <see cref="DynamicMeasurementRecord"/>.
    /// Cascade delete dal MeasurementType; i Values figli vengono eliminati
    /// a cascata tramite <see cref="DynamicMeasurementValueConfiguration"/>.
    /// </summary>
    public class DynamicMeasurementRecordConfiguration : IEntityTypeConfiguration<DynamicMeasurementRecord>
    {
        public void Configure(EntityTypeBuilder<DynamicMeasurementRecord> builder)
        {
            builder.ToTable("DynamicMeasurementRecords");

            builder.HasOne(r => r.MeasurementType)
                .WithMany()
                .HasForeignKey(r => r.MeasurementTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Cliente)
                .WithMany()
                .HasForeignKey(r => r.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => new { r.ClienteId, r.MeasurementTypeId });
        }
    }
}
