using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;
using MisureRicci.Services;
using MisureRicci.Data.Configurations;

namespace MisureRicci.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenantService? _tenantService;
        private readonly int? _currentTenantId;
        private readonly bool _isTenantAdmin;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService? tenantService = null)
            : base(options)
        {
            _tenantService = tenantService;
            _currentTenantId = _tenantService?.GetCurrentNegozioId();
            // In testing or design-time (migrations), if no service is provided, we default to Admin=true
            // to ensure all data is visible/manageable.
            _isTenantAdmin = _tenantService?.IsAdmin() ?? true;
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

            // Global Query Filters per Multi-Tenancy
            // Use local fields initialized in constructor to ensure proper expression translation
            builder.Entity<CommessaSartoriale>().HasQueryFilter(c => 
                _isTenantAdmin || (c.NegozioId == _currentTenantId));
            
            builder.Entity<MisureCliente>().HasQueryFilter(m => 
                _isTenantAdmin || (m.Cliente != null && m.Cliente.NegozioId == _currentTenantId));
            
            builder.Entity<Cliente>().HasQueryFilter(c => 
                _isTenantAdmin || (c.NegozioId == _currentTenantId));
            
            builder.Entity<DynamicMeasurementRecord>().HasQueryFilter(r => 
                _isTenantAdmin || (r.Cliente != null && r.Cliente.NegozioId == _currentTenantId));

            builder.Entity<DynamicMeasurementValue>().HasQueryFilter(v =>
                _isTenantAdmin || (v.DynamicMeasurementRecord != null && v.DynamicMeasurementRecord.Cliente != null && v.DynamicMeasurementRecord.Cliente.NegozioId == _currentTenantId));

            builder.Entity<CommessaEvento>().HasQueryFilter(e =>
                _isTenantAdmin || (e.CommessaSartoriale != null && e.CommessaSartoriale.NegozioId == _currentTenantId));

            builder.Entity<CommessaMisuraLink>().HasQueryFilter(l =>
                _isTenantAdmin || (l.CommessaSartoriale != null && l.CommessaSartoriale.NegozioId == _currentTenantId));

            // Negozi e Utenti: NON filtrati per tenant.
            // - Negozi: visibili a tutti (dropdown, selezione, ecc.)
            // - ApplicationUser: filtrati dalla [Authorize(Roles)] nei controller, non dal DbContext

            // Legacy measurement filters
            builder.Entity<GiaccaMeasurement>().HasQueryFilter(m => _isTenantAdmin || (m.Cliente != null && m.Cliente.NegozioId == _currentTenantId));
            builder.Entity<PantaloneMeasurement>().HasQueryFilter(m => _isTenantAdmin || (m.Cliente != null && m.Cliente.NegozioId == _currentTenantId));
            builder.Entity<GiletMeasurement>().HasQueryFilter(m => _isTenantAdmin || (m.Cliente != null && m.Cliente.NegozioId == _currentTenantId));
            builder.Entity<CamiciaMeasurement>().HasQueryFilter(m => _isTenantAdmin || (m.Cliente != null && m.Cliente.NegozioId == _currentTenantId));
            builder.Entity<AbitoCompletoMeasurement>().HasQueryFilter(m => _isTenantAdmin || (m.Cliente != null && m.Cliente.NegozioId == _currentTenantId));
            builder.Entity<MaglieMeasurement>().HasQueryFilter(m => _isTenantAdmin || (m.Cliente != null && m.Cliente.NegozioId == _currentTenantId));
            builder.Entity<OutdoorMeasurement>().HasQueryFilter(m => _isTenantAdmin || (m.Cliente != null && m.Cliente.NegozioId == _currentTenantId));
            builder.Entity<ScarpeMeasurement>().HasQueryFilter(m => _isTenantAdmin || (m.Cliente != null && m.Cliente.NegozioId == _currentTenantId));
            builder.Entity<CravattaMeasurement>().HasQueryFilter(m => _isTenantAdmin || (m.Cliente != null && m.Cliente.NegozioId == _currentTenantId));
            builder.Entity<CinturaMeasurement>().HasQueryFilter(m => _isTenantAdmin || (m.Cliente != null && m.Cliente.NegozioId == _currentTenantId));
        }
    }
}
