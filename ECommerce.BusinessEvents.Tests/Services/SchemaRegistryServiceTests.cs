using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Service;

namespace ECommerce.BusinessEvents.Tests.Services
{
    public class SchemaRegistryServiceTests : IDisposable
    {
        private readonly BusinessEventDbContext _context;
        private readonly SchemaRegistryService _service;

        public SchemaRegistryServiceTests()
        {
            var options = new DbContextOptionsBuilder<BusinessEventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BusinessEventDbContext(options);
            _service = new SchemaRegistryService(_context);
        }

        [Fact]
        public async Task AddSchemaAsync_ShouldAddSchemaToDatabase()
        {
            // Arrange
            var entityType = "Customer";
            var version = 1;
            var schemaDefinition = "{ \"type\": \"object\" }";

            // Act
            await _service.AddSchemaAsync(entityType, version, schemaDefinition);

            // Assert
            var schema = await _context.SchemaVersions
                .FirstOrDefaultAsync(sv => sv.EntityType == entityType && sv.Version == version);

            Assert.NotNull(schema);
            Assert.Equal(entityType, schema.EntityType);
            Assert.Equal(version, schema.Version);
            Assert.Equal(schemaDefinition, schema.SchemaDefinition);
        }

        [Fact]
        public async Task GetSchemaAsync_ShouldReturnCorrectSchema()
        {
            // Arrange
            var entityType = "Customer";
            var version = 1;
            var schemaDefinition = "{ \"type\": \"object\" }";
            await _service.AddSchemaAsync(entityType, version, schemaDefinition);

            // Act
            var result = await _service.GetSchemaAsync(entityType, version);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entityType, result.EntityType);
            Assert.Equal(version, result.Version);
            Assert.Equal(schemaDefinition, result.SchemaDefinition);
        }

        [Fact]
        public async Task GetSchemaAsync_ShouldReturnNullForNonExistentSchema()
        {
            // Act
            var result = await _service.GetSchemaAsync("NonExistent", 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetLatestSchemaAsync_ShouldReturnHighestVersionSchema()
        {
            // Arrange
            var entityType = "Customer";
            await _service.AddSchemaAsync(entityType, 1, "{ \"version\": 1 }");
            await _service.AddSchemaAsync(entityType, 3, "{ \"version\": 3 }");
            await _service.AddSchemaAsync(entityType, 2, "{ \"version\": 2 }");

            // Act
            var result = await _service.GetLatestSchemaAsync(entityType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Version);
            Assert.Equal("{ \"version\": 3 }", result.SchemaDefinition);
        }

        [Fact]
        public async Task GetLatestSchemaAsync_ShouldReturnNullForNonExistentEntity()
        {
            // Act
            var result = await _service.GetLatestSchemaAsync("NonExistent");

            // Assert
            Assert.Null(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
