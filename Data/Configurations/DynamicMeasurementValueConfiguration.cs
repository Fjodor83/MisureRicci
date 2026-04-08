using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MisureRicci.Models;

namespace MisureRicci.Data.Configurations
{
    /// <summary>
    /// Configurazione EF Core per <see cref="DynamicMeasurementValue"/>.
    /// Cascade delete dal Record padre; Restrict sulla FieldDefinition
    /// per impedire la rimozione accidentale di una definizione campo ancora in uso.
    /// </summary>
    public class DynamicMeasurementValueConfiguration : IEntityTypeConfiguration<DynamicMeasurementValue>
    {
        public void Configure(EntityTypeBuilder<DynamicMeasurementValue> builder)
        {
            builder.ToTable("DynamicMeasurementValues");

            builder.HasOne(v => v.MeasurementFieldDefinition)
                .WithMany(f => f.Values)
                .HasForeignKey(v => v.MeasurementFieldDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.DynamicMeasurementRecord)
                .WithMany(r => r.Values)
                .HasForeignKey(v => v.DynamicMeasurementRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(v => new { v.DynamicMeasurementRecordId, v.MeasurementFieldDefinitionId })
                .IsUnique();
        }
    }
}
