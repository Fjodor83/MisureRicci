using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MisureRicci.Models;

namespace MisureRicci.Data.Configurations
{
    /// <summary>
    /// Configurazione EF Core per <see cref="MeasurementFieldDefinition"/>.
    /// Indice univoco composito su (MeasurementTypeId, NomeCampo) per
    /// impedire campi duplicati all'interno della stessa tipologia.
    /// </summary>
    public class DynamicFieldConfiguration : IEntityTypeConfiguration<MeasurementFieldDefinition>
    {
        public void Configure(EntityTypeBuilder<MeasurementFieldDefinition> builder)
        {
            builder.ToTable("DynamicFieldDefinitions");

            builder.Property(f => f.NomeCampo)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(f => f.Etichetta)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(f => new { f.MeasurementTypeId, f.NomeCampo }).IsUnique();
        }
    }
}
