using Microsoft.EntityFrameworkCore;
using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Services;
using ECommerce.Modules.Customers.Domain;

namespace ECommerce.BusinessEvents.Tests.Services
{
    public class EventTrackingServiceTests : IDisposable
    {
        private readonly BusinessEventDbContext _context;
        private readonly SchemaRegistryService _schemaRegistry;
        private readonly EventTrackingService _eventTracker;

        public EventTrackingServiceTests()
        {
            var options = new DbContextOptionsBuilder<BusinessEventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BusinessEventDbContext(options);
            _schemaRegistry = new SchemaRegistryService(_context);
            _eventTracker = new EventTrackingService(_context, _schemaRegistry);

            // Set up schema for testing
            InitializeTestSchema().Wait();
        }

        private async Task InitializeTestSchema()
        {
            string customerSchema = @"{
                ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
                ""$id"": ""https://example.com/schemas/customer/v1"",
                ""title"": ""Customer"",
                ""type"": ""object"",
                ""properties"": {
                    ""Id"": { ""type"": ""string"" },
                    ""Name"": { ""type"": ""string"" },
                    ""Email"": { ""type"": ""string"", ""format"": ""email"" }
                },
                ""required"": [""Id"", ""Name"", ""Email""]
            }";

            await _schemaRegistry.AddSchemaAsync("Customer", 1, customerSchema);
        }

        [Fact]
        public async Task TrackEventAsync_ValidData_SavesEventToDatabase()
        {
            // Arrange
            Customer customer = new Customer("John Doe", "john.doe@example.com");

            // Act
            await _eventTracker.TrackEventAsync(
                entityType: "Customer",
                entityId: 1,
                eventType: "CustomerCreated",
                actorId: "test-user",
                entityData: customer);

            // Assert
            var savedEvent = await _context.BusinessEvents.FirstOrDefaultAsync();
            Assert.NotNull(savedEvent);
            Assert.Equal("Customer", savedEvent.EntityType);
            Assert.Equal(1, savedEvent.EntityId);
            Assert.Equal("CustomerCreated", savedEvent.EventType);
            Assert.Equal("test-user", savedEvent.ActorId);
            Assert.Equal(1, savedEvent.SchemaVersion);
            Assert.Contains("john.doe@example.com", savedEvent.EntityData);
        }

        [Fact]
        public async Task TrackEventAsync_InvalidData_ThrowsException()
        {
            // Arrange
            var invalidCustomer = new Customer("john doe", "wrong-email-format");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _eventTracker.TrackEventAsync(
                    entityType: "Customer",
                    entityId: 1,
                    eventType: "CustomerCreated",
                    actorId: "test-user",
                    entityData: invalidCustomer));

            // Verify no events were saved
            Assert.Empty(await _context.BusinessEvents.ToListAsync());
        }

        [Fact]
        public async Task TrackEventAsync_NonExistentSchema_ThrowsException()
        {
            // Arrange
            var vehicle = new { Id = 1, Make = "Toyota", Model = "Corolla" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _eventTracker.TrackEventAsync(
                    entityType: "Vehicle",  // No schema exists for Vehicle
                    entityId: 1,
                    eventType: "VehicleCreated",
                    actorId: "test-user",
                    entityData: vehicle));

            // Verify no events were saved
            Assert.Empty(await _context.BusinessEvents.ToListAsync());
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
