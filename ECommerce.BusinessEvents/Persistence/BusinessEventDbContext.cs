using ECommerce.BusinessEvents.Domain;
using ECommerce.BusinessEvents.Domain.Schemas;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Domain.BusinessEvents;

namespace ECommerce.BusinessEvents.Persistence
{
    public class BusinessEventDbContext : DbContext
    {
        public BusinessEventDbContext(DbContextOptions<BusinessEventDbContext> options)
            : base(options)
        {
        }

        public DbSet<BusinessEvent> BusinessEvents { get; set; }
        public DbSet<SchemaVersion> SchemaVersions { get; set; }
        public DbSet<BusinessEventMetadata> BusinessEventMetadata { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // BusinessEvent configuration
            modelBuilder.Entity<BusinessEvent>()
                .HasKey(be => be.EventId);

            modelBuilder.Entity<BusinessEvent>()
                .HasIndex(be => new { be.EntityType, be.EntityId, be.EventTimestamp });

            // SchemaVersion configuration
            modelBuilder.Entity<SchemaVersion>()
                .HasKey(sv => new { sv.EntityType, sv.Version });

            // Seed Customer and Product schemas
            modelBuilder.Entity<SchemaVersion>().HasData(
                CustomerSchemaVersions.All.ToArray()
                    .Concat(ProductSchemaVersions.All.ToArray())
                    .ToArray()
            );

            // BusinessEventMetadata configuration
            modelBuilder.Entity<BusinessEventMetadata>()
                .HasKey(bem => new { bem.EventId, bem.MetadataKey });

            modelBuilder.Entity<BusinessEventMetadata>()
                .HasIndex(bem => new { bem.EntityType, bem.EntityId, bem.MetadataKey })
                .HasDatabaseName("IX_BusinessEventMetadata_Entity");

            modelBuilder.Entity<BusinessEventMetadata>()
                .HasIndex(bem => new { bem.MetadataKey, bem.MetadataValue })
                .HasDatabaseName("IX_BusinessEventMetadata_Key_Value");

            modelBuilder.Entity<BusinessEventMetadata>()
                .HasOne(bem => bem.BusinessEvent)
                .WithMany()
                .HasForeignKey(bem => bem.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // ...additional configuration as needed...
        }
    }
}
