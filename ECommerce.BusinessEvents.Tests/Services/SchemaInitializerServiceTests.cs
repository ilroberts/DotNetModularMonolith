using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Services;
using ECommerce.BusinessEvents.Service;
using ModularMonolith.Domain.BusinessEvents;

namespace ECommerce.BusinessEvents.Tests.Services
{
    public class SchemaInitializerServiceTests : IDisposable
    {
        private readonly BusinessEventDbContext _context;
        private readonly SchemaRegistryService _schemaRegistry;
        private readonly SchemaInitializerService _initializerService;

        public SchemaInitializerServiceTests()
        {
            var options = new DbContextOptionsBuilder<BusinessEventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BusinessEventDbContext(options);
            _schemaRegistry = new SchemaRegistryService(_context);
            _initializerService = new SchemaInitializerService(_schemaRegistry);
        }

        [Fact]
        public async Task InitializeDefaultSchemasAsync_ShouldAddCustomerSchema()
        {
            // Arrange
            const string customerEntityType = "Customer";
            const int version = 1;

            // Act
            await _initializerService.InitializeDefaultSchemasAsync();

            // Assert
            var schemas = await _context.SchemaVersions.ToListAsync();
            Assert.NotEmpty(schemas);

            var customerSchema = await _context.SchemaVersions
                .FirstOrDefaultAsync(s => s.EntityType == customerEntityType && s.Version == version);

            Assert.NotNull(customerSchema);
            Assert.Contains("Customer", customerSchema.SchemaDefinition);
            Assert.Contains("$schema", customerSchema.SchemaDefinition);
        }

        [Fact]
        public async Task InitializeDefaultSchemasAsync_ShouldNotDuplicateSchemasOnSecondRun()
        {
            // Arrange
            const string customerEntityType = "Customer";

            // First run to add schemas
            await _initializerService.InitializeDefaultSchemasAsync();

            // Get count after first initialization
            var initialCount = await _context.SchemaVersions.CountAsync();

            // Act
            await _initializerService.InitializeDefaultSchemasAsync();

            // Assert
            var finalCount = await _context.SchemaVersions.CountAsync();
            Assert.Equal(initialCount, finalCount);

            var customerSchemas = await _context.SchemaVersions
                .Where(s => s.EntityType == customerEntityType)
                .ToListAsync();

            Assert.Single(customerSchemas); // Should still have just one customer schema
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
