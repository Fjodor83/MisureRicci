using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MisureRicci.Models;

namespace MisureRicci.Data.Configurations
{
    public class CommessaMisuraLinkConfiguration : IEntityTypeConfiguration<CommessaMisuraLink>
    {
        public void Configure(EntityTypeBuilder<CommessaMisuraLink> builder)
        {
            builder.HasOne(cl => cl.CommessaSartoriale)
                .WithMany(c => c.MisureCollegate)
                .HasForeignKey(cl => cl.CommessaSartorialeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cl => cl.MisuraCliente)
                .WithMany()
                .HasForeignKey(cl => cl.MisuraClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class DynamicMeasurementConfiguration : IEntityTypeConfiguration<MeasurementType>
    {
        public void Configure(EntityTypeBuilder<MeasurementType> builder)
        {
            builder.HasIndex(t => t.Nome).IsUnique();
        }
    }

    public class DynamicFieldConfiguration : IEntityTypeConfiguration<MeasurementFieldDefinition>
    {
        public void Configure(EntityTypeBuilder<MeasurementFieldDefinition> builder)
        {
            builder.HasIndex(f => new { f.MeasurementTypeId, f.NomeCampo }).IsUnique();
        }
    }
}
