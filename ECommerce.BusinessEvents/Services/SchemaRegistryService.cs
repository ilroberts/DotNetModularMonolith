using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Domain.BusinessEvents;
using ECommerce.BusinessEvents.Persistence;

namespace ECommerce.BusinessEvents.Service
{
    public class SchemaRegistryService
    {
        private readonly BusinessEventDbContext _dbContext;

        public SchemaRegistryService(BusinessEventDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SchemaVersion?> GetSchemaAsync(string entityType, int version)
        {
            return await _dbContext.SchemaVersions
                .FirstOrDefaultAsync(sv => sv.EntityType == entityType && sv.Version == version);
        }

        public async Task AddSchemaAsync(string entityType, int version, string schemaDefinition)
        {
            var schemaVersion = new SchemaVersion
            {
                EntityType = entityType,
                Version = version,
                SchemaDefinition = schemaDefinition,
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.SchemaVersions.Add(schemaVersion);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<SchemaVersion?> GetLatestSchemaAsync(string entityType)
        {
            return await _dbContext.SchemaVersions
                .Where(sv => sv.EntityType == entityType)
                .OrderByDescending(sv => sv.Version)
                .FirstOrDefaultAsync();
        }
    }
}
