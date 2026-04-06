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

    public class DynamicMeasurementRecordConfiguration : IEntityTypeConfiguration<DynamicMeasurementRecord>
    {
        public void Configure(EntityTypeBuilder<DynamicMeasurementRecord> builder)
        {
            builder.HasOne(r => r.MeasurementType)
                .WithMany()
                .HasForeignKey(r => r.MeasurementTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class DynamicMeasurementValueConfiguration : IEntityTypeConfiguration<DynamicMeasurementValue>
    {
        public void Configure(EntityTypeBuilder<DynamicMeasurementValue> builder)
        {
            builder.HasOne(v => v.MeasurementFieldDefinition)
                .WithMany(f => f.Values)
                .HasForeignKey(v => v.MeasurementFieldDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.DynamicMeasurementRecord)
                .WithMany(r => r.Values)
                .HasForeignKey(v => v.DynamicMeasurementRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class AbitoCompletoMeasurementConfiguration : IEntityTypeConfiguration<AbitoCompletoMeasurement>
    {
        public void Configure(EntityTypeBuilder<AbitoCompletoMeasurement> builder)
        {
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

    /// <summary>
    /// ClientCode: su PostgreSQL è una colonna normale (non computed).
    /// Il valore viene generato da WebApplicationExtensions durante la creazione del cliente.
    /// </summary>
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            // ClientCode non è più una colonna calcolata SQL Server.
            // Viene impostato esplicitamente dal servizio prima del salvataggio.
            builder.Property(c => c.ClientCode)
                .HasMaxLength(20)
                .IsRequired(false);
        }
    }
}
