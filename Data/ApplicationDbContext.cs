using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;

namespace MisureRicci.Data
{
    public class ApplicationDbContext : DbContext
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




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<MisureCliente>().ToTable("RegistroMisure");
            
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
        }


    }
}
