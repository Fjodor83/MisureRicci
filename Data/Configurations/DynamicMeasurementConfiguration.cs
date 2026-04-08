using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MisureRicci.Models;

namespace MisureRicci.Data.Configurations
{
    /// <summary>
    /// Configurazione EF Core per <see cref="MeasurementType"/>.
    /// Nome univoco per evitare duplicati di tipologie misura.
    /// </summary>
    public class DynamicMeasurementConfiguration : IEntityTypeConfiguration<MeasurementType>
    {
        public void Configure(EntityTypeBuilder<MeasurementType> builder)
        {
            builder.ToTable("DynamicMeasurementTypes");

            builder.Property(t => t.Nome)
                .HasMaxLength(80)
                .IsRequired();

            builder.HasIndex(t => t.Nome).IsUnique();

            builder.HasMany(t => t.Campi)
                .WithOne(f => f.MeasurementType)
                .HasForeignKey(f => f.MeasurementTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
