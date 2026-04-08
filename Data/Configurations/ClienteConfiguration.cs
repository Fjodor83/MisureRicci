using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MisureRicci.Models;

namespace MisureRicci.Data.Configurations
{
    /// <summary>
    /// Configurazione EF Core per <see cref="Cliente"/>.
    /// ClientCode è una stringa nullable max 20, generata dal servizio.
    /// Indice composito su (NegozioId, Cognome, Nome) per le query paginate.
    /// Indice univoco su Email per impedire duplicati.
    /// </summary>
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Clienti");

            builder.Property(c => c.ClientCode)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.HasIndex(c => c.Email).IsUnique();

            builder.HasIndex(c => new { c.NegozioId, c.Cognome, c.Nome })
                .HasDatabaseName("IX_Clienti_Negozio_Cognome_Nome");

            builder.HasOne(c => c.Negozio)
                .WithMany()
                .HasForeignKey(c => c.NegozioId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
