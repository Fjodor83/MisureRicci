using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MisureRicci.Models;

namespace MisureRicci.Data.Configurations
{
    /// <summary>
    /// Configurazione EF Core per <see cref="AbitoCompletoMeasurement"/>.
    /// Mappa le navigation properties Giacca e Pantalone con FK shadow properties
    /// e Restrict delete per evitare eliminazione accidentale delle sotto-misure.
    /// </summary>
    public class AbitoCompletoMeasurementConfiguration : IEntityTypeConfiguration<AbitoCompletoMeasurement>
    {
        public void Configure(EntityTypeBuilder<AbitoCompletoMeasurement> builder)
        {
            builder.ToTable("MisureAbitoCompleto");

            builder.HasOne(m => m.Giacca)
                .WithMany()
                .HasForeignKey("GiaccaId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Pantalone)
                .WithMany()
                .HasForeignKey("PantaloneId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
