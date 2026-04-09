using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MisureRicci.Models;

namespace MisureRicci.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EntityName)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.EntityId)
                .HasMaxLength(128);

            builder.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.UserId)
                .HasMaxLength(450);

            builder.HasIndex(x => x.Timestamp);
            builder.HasIndex(x => new { x.EntityName, x.EntityId });
        }
    }
}
