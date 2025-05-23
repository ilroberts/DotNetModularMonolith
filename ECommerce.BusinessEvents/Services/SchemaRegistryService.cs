using Microsoft.EntityFrameworkCore;
using ModularMonolith.Domain.BusinessEvents;
using ECommerce.BusinessEvents.Persistence;

namespace ECommerce.BusinessEvents.Services
{
    public class SchemaRegistryService(BusinessEventDbContext dbContext)
    {
        public async Task<SchemaVersion?> GetSchemaAsync(string entityType, int version)
        {
            return await dbContext.SchemaVersions
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

            dbContext.SchemaVersions.Add(schemaVersion);
            await dbContext.SaveChangesAsync();
        }

        public async Task<SchemaVersion?> GetLatestSchemaAsync(string entityType)
        {
            return await dbContext.SchemaVersions
                .Where(sv => sv.EntityType == entityType)
                .OrderByDescending(sv => sv.Version)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SchemaVersion>> GetSchemaVersionsAsync(string entityType)
        {
            return await dbContext.SchemaVersions
                .Where(sv => sv.EntityType == entityType)
                .OrderBy(sv => sv.Version)
                .ToListAsync();
        }

        public async Task<string?> GetSchemaDefinitionAsync(string entityType, int version)
        {
            var schema = await GetSchemaAsync(entityType, version);
            return schema?.SchemaDefinition;
        }
    }
}
