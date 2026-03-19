using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;

namespace MisureRicci.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<GiaccaMeasurement> MisureGiacca { get; set; }
        public DbSet<PantaloneMeasurement> MisurePantalone { get; set; }
        public DbSet<AbitoCompletoMeasurement> MisureAbitoCompleto { get; set; }
        public DbSet<GiletMeasurement> MisureGilet { get; set; }
        public DbSet<MaglieMeasurement> MisureMaglie { get; set; }
        public DbSet<OutdoorMeasurement> MisureOutdoor { get; set; }
        public DbSet<CamiciaMeasurement> MisureCamicia { get; set; }
        public DbSet<ScarpeMeasurement> MisureScarpe { get; set; }
        public DbSet<CravattaMeasurement> MisureCravatta { get; set; }
        public DbSet<CinturaMeasurement> MisureCintura { get; set; }



        public DbSet<Cliente> Clienti { get; set; }
        public DbSet<Negozio> Negozi { get; set; }
        public DbSet<Utente> Utenti { get; set; }
        public DbSet<MisureCliente> RegistroMisure { get; set; }
        public DbSet<MeasurementType> MeasurementTypes { get; set; }
        public DbSet<MeasurementFieldDefinition> MeasurementFieldDefinitions { get; set; }
        public DbSet<DynamicMeasurementRecord> DynamicMeasurementRecords { get; set; }
        public DbSet<DynamicMeasurementValue> DynamicMeasurementValues { get; set; }
        public DbSet<CommessaSartoriale> CommesseSartoriali { get; set; }
        public DbSet<CommessaEvento> CommesseEventi { get; set; }
        public DbSet<CommessaMisuraLink> CommesseMisureLinks { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<MisureCliente>().ToTable("RegistroMisure");
            modelBuilder.Entity<MisureCliente>()
                .HasIndex(x => new { x.ClienteId, x.DataCreazione });
            
            // Fix for multiple cascade paths in AbitoCompleto
            modelBuilder.Entity<AbitoCompletoMeasurement>(entity =>
            {
                entity.HasOne(a => a.Giacca)
                      .WithMany()
                      .HasForeignKey("GiaccaId")
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Pantalone)
                      .WithMany()
                      .HasForeignKey("PantaloneId")
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Ensure all measurement tables have clear Italian names if not already set by DbSet
            modelBuilder.Entity<GiaccaMeasurement>().ToTable("MisureGiacca");
            modelBuilder.Entity<PantaloneMeasurement>().ToTable("MisurePantalone");
            modelBuilder.Entity<AbitoCompletoMeasurement>().ToTable("MisureAbitoCompleto");
            modelBuilder.Entity<GiletMeasurement>().ToTable("MisureGilet");
            modelBuilder.Entity<MaglieMeasurement>().ToTable("MisureMaglie");
            modelBuilder.Entity<OutdoorMeasurement>().ToTable("MisureOutdoor");
            modelBuilder.Entity<CamiciaMeasurement>().ToTable("MisureCamicia");
            modelBuilder.Entity<ScarpeMeasurement>().ToTable("MisureScarpe");
            modelBuilder.Entity<CravattaMeasurement>().ToTable("MisureCravatta");
            modelBuilder.Entity<CinturaMeasurement>().ToTable("MisureCintura");
            modelBuilder.Entity<Cliente>().ToTable("Clienti");
            modelBuilder.Entity<Negozio>().ToTable("Negozi");
            modelBuilder.Entity<Utente>().ToTable("Utenti");

            modelBuilder.Entity<MeasurementType>(entity =>
            {
                entity.ToTable("MeasurementTypes");
                entity.HasIndex(x => x.Nome).IsUnique();
            });

            modelBuilder.Entity<MeasurementFieldDefinition>(entity =>
            {
                entity.ToTable("MeasurementFieldDefinitions");
                entity.Property(x => x.Gruppo).HasMaxLength(80);
                entity.Property(x => x.HelpText).HasMaxLength(160);
                entity.HasOne(x => x.MeasurementType)
                    .WithMany(x => x.Campi)
                    .HasForeignKey(x => x.MeasurementTypeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(x => new { x.MeasurementTypeId, x.NomeCampo }).IsUnique();
            });

            modelBuilder.Entity<DynamicMeasurementRecord>(entity =>
            {
                entity.ToTable("DynamicMeasurementRecords");
                entity.HasOne(x => x.Cliente)
                    .WithMany()
                    .HasForeignKey(x => x.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.MeasurementType)
                    .WithMany()
                    .HasForeignKey(x => x.MeasurementTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<DynamicMeasurementValue>(entity =>
            {
                entity.ToTable("DynamicMeasurementValues");
                entity.HasOne(x => x.DynamicMeasurementRecord)
                    .WithMany(x => x.Values)
                    .HasForeignKey(x => x.DynamicMeasurementRecordId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.MeasurementFieldDefinition)
                    .WithMany(x => x.Values)
                    .HasForeignKey(x => x.MeasurementFieldDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CommessaSartoriale>(entity =>
            {
                entity.ToTable("CommesseSartoriali");
                entity.Property(x => x.TipoCapo).HasMaxLength(80);
                entity.Property(x => x.Tessuto).HasMaxLength(120);
                entity.Property(x => x.Collezione).HasMaxLength(120);
                entity.Property(x => x.NoteInterne).HasMaxLength(2000);
                entity.HasIndex(x => x.CommessaCode).IsUnique();
                entity.HasIndex(x => new { x.Stato, x.DataConsegnaPrevista });
                entity.HasIndex(x => new { x.ClienteId, x.DataApertura });

                entity.HasOne(x => x.Cliente)
                    .WithMany()
                    .HasForeignKey(x => x.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Negozio)
                    .WithMany()
                    .HasForeignKey(x => x.NegozioId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(x => x.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(x => x.Eventi)
                    .WithOne(x => x.CommessaSartoriale)
                    .HasForeignKey(x => x.CommessaSartorialeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CommessaEvento>(entity =>
            {
                entity.ToTable("CommesseEventi");
                entity.Property(x => x.TipoEvento).HasMaxLength(40);
                entity.Property(x => x.Descrizione).HasMaxLength(1000);
                entity.HasIndex(x => x.CommessaSartorialeId);

                entity.HasOne(x => x.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CommessaMisuraLink>(entity =>
            {
                entity.ToTable("CommesseMisureLinks");
                entity.HasIndex(x => new { x.CommessaSartorialeId, x.MisuraClienteId }).IsUnique();

                entity.HasOne(x => x.CommessaSartoriale)
                    .WithMany(x => x.MisureCollegate)
                    .HasForeignKey(x => x.CommessaSartorialeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.MisuraCliente)
                    .WithMany()
                    .HasForeignKey(x => x.MisuraClienteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.LinkedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.LinkedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var addedClients = ChangeTracker.Entries<Cliente>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .ToList();

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            bool needsSecondSave = false;
            foreach (var cliente in addedClients)
            {
                if (string.IsNullOrEmpty(cliente.ClientCode))
                {
                    cliente.ClientCode = $"SR-{DateTime.Now.Year}-{cliente.Id:D5}";
                    needsSecondSave = true;
                }
            }

            if (needsSecondSave)
            {
                await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }

            return result;
        }
        
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return SaveChangesAsync(true, cancellationToken);
        }
    }
}
