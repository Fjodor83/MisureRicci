using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;
using MisureRicci.Services;
using MisureRicci.Data.Configurations;

namespace MisureRicci.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Negozio> Negozi { get; set; } = default!;
        public DbSet<Cliente> Clienti { get; set; } = default!;
        public DbSet<MisureCliente> Misure { get; set; } = default!;
        public DbSet<CommessaSartoriale> Commissioni { get; set; } = default!;
        public DbSet<CommessaEvento> CommissioniEventi { get; set; } = default!;
        public DbSet<CommessaMisuraLink> CommissioniMisureLinks { get; set; } = default!;

        public DbSet<MeasurementType> DynamicMeasurementTypes { get; set; } = default!;
        public DbSet<MeasurementFieldDefinition> DynamicFieldDefinitions { get; set; } = default!;
        public DbSet<DynamicMeasurementRecord> DynamicMeasurementRecords { get; set; } = default!;
        public DbSet<DynamicMeasurementValue> DynamicMeasurementValues { get; set; } = default!;

        // Legati a tabelle fisiche di misura legacy
        public DbSet<GiaccaMeasurement> MisureGiacca { get; set; } = default!;
        public DbSet<PantaloneMeasurement> MisurePantalone { get; set; } = default!;
        public DbSet<GiletMeasurement> MisureGilet { get; set; } = default!;
        public DbSet<CamiciaMeasurement> MisureCamicia { get; set; } = default!;
        public DbSet<AbitoCompletoMeasurement> MisureAbitoCompleto { get; set; } = default!;
        public DbSet<MaglieMeasurement> MisureMaglie { get; set; } = default!;
        public DbSet<OutdoorMeasurement> MisureOutdoor { get; set; } = default!;
        public DbSet<ScarpeMeasurement> MisureScarpe { get; set; } = default!;
        public DbSet<CravattaMeasurement> MisureCravatta { get; set; } = default!;
        public DbSet<CinturaMeasurement> MisureCintura { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply configurations from separate classes
            builder.ApplyConfiguration(new CommessaMisuraLinkConfiguration());
            builder.ApplyConfiguration(new DynamicMeasurementConfiguration());
            builder.ApplyConfiguration(new DynamicFieldConfiguration());

            builder.Entity<CommessaSartoriale>().ToTable("CommissioniSartoriali");
            builder.Entity<MisureCliente>().ToTable("RegistroMisure");

            // Mapping legacy tables
            builder.Entity<GiaccaMeasurement>().ToTable("MisureGiacca");
            builder.Entity<PantaloneMeasurement>().ToTable("MisurePantalone");
            builder.Entity<GiletMeasurement>().ToTable("MisureGilet");
            builder.Entity<CamiciaMeasurement>().ToTable("MisureCamicia");
            builder.Entity<AbitoCompletoMeasurement>().ToTable("MisureAbitoCompleto");
            builder.Entity<MaglieMeasurement>().ToTable("MisureMaglie");
            builder.Entity<OutdoorMeasurement>().ToTable("MisureOutdoor");
            builder.Entity<ScarpeMeasurement>().ToTable("MisureScarpe");
            builder.Entity<CravattaMeasurement>().ToTable("MisureCravatta");
            builder.Entity<CinturaMeasurement>().ToTable("MisureCintura");

            // Computed Column handling for ClientCode
            builder.Entity<Cliente>()
                .Property(c => c.ClientCode)
                .ValueGeneratedOnAddOrUpdate()
                .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            // Note: Multi-tenancy filters have been removed from DbContext to avoid stale state capture 
            // and circular dependency issues. Scoping is now enforced exclusively in the service layer.
        }
    }
}
