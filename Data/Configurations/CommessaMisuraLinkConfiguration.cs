using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MisureRicci.Models;

namespace MisureRicci.Data.Configurations
{
    /// <summary>
    /// Configurazione EF Core per <see cref="CommessaMisuraLink"/>.
    /// La commessa cascade-deleta i suoi link, ma il link NON elimina la misura sottostante.
    /// </summary>
    public class CommessaMisuraLinkConfiguration : IEntityTypeConfiguration<CommessaMisuraLink>
    {
        public void Configure(EntityTypeBuilder<CommessaMisuraLink> builder)
        {
            builder.ToTable("CommissioniMisureLinks");

            builder.HasOne(cl => cl.CommessaSartoriale)
                .WithMany(c => c.MisureCollegate)
                .HasForeignKey(cl => cl.CommessaSartorialeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cl => cl.MisuraCliente)
                .WithMany()
                .HasForeignKey(cl => cl.MisuraClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(cl => new { cl.CommessaSartorialeId, cl.MisuraClienteId })
                .IsUnique();
        }
    }
}
