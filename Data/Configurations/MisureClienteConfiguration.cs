using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MisureRicci.Models;

namespace MisureRicci.Data.Configurations
{
    public class MisureClienteConfiguration : IEntityTypeConfiguration<MisureCliente>
    {
        public void Configure(EntityTypeBuilder<MisureCliente> builder)
        {
            builder.ToTable("RegistroMisure");

            builder.Property(m => m.TipoMisura)
                .HasMaxLength(80);

            builder.HasIndex(m => new { m.ClienteId, m.TipoMisura })
                .HasDatabaseName("IX_RegistroMisure_ClienteId_TipoMisura");
        }
    }
}
