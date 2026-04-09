using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MisureRicci.Models;

namespace MisureRicci.Data.Configurations
{
    public class FabricConfiguration : IEntityTypeConfiguration<Fabric>
    {
        public void Configure(EntityTypeBuilder<Fabric> builder)
        {
            builder.ToTable("Fabrics");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Nome)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(f => f.Descrizione)
                .HasMaxLength(250);

            builder.Property(f => f.Composizione)
                .HasMaxLength(100);

            builder.Property(f => f.IsActive)
                .HasDefaultValue(true);

            builder.Property(f => f.CreatedAt);

            builder.HasIndex(f => f.Nome)
                .IsUnique()
                .HasDatabaseName("IX_Fabrics_Nome");
        }
    }
}
