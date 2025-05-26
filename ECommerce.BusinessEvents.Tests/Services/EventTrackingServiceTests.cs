using Microsoft.EntityFrameworkCore;
using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Services;
using ECommerce.Modules.Customers.Domain;
using ECommerce.BusinessEvents.Infrastructure.Validators;
using Moq;

namespace ECommerce.BusinessEvents.Tests.Services
{
    public class EventTrackingServiceTests : IDisposable
    {
        private readonly BusinessEventDbContext _context;
        private readonly SchemaRegistryService _schemaRegistry;
        private readonly Mock<IJsonSchemaValidator> _schemaValidatorMock;
        private readonly EventTrackingService _eventTracker;

        public EventTrackingServiceTests()
        {
            var options = new DbContextOptionsBuilder<BusinessEventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BusinessEventDbContext(options);
            _schemaRegistry = new SchemaRegistryService(_context);
            _schemaValidatorMock = new Mock<IJsonSchemaValidator>();
            // Add a mock logger for EventTrackingService
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EventTrackingService>>();
            _eventTracker = new EventTrackingService(_context, _schemaRegistry, _schemaValidatorMock.Object, loggerMock.Object);

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
            _schemaValidatorMock
                .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
                .Verifiable();

            // Act
            await _eventTracker.TrackEventAsync(
                entityType: "Customer",
                entityId: "1",
                eventType: "CustomerCreated",
                actorId: "test-user",
                actorType: "User",
                entityData: customer);

            // Assert
            var savedEvent = await _context.BusinessEvents.FirstOrDefaultAsync();
            Assert.NotNull(savedEvent);
            Assert.Equal("Customer", savedEvent.EntityType);
            Assert.Equal("1", savedEvent.EntityId);
            Assert.Equal("CustomerCreated", savedEvent.EventType);
            Assert.Equal("test-user", savedEvent.ActorId);
            Assert.Equal("User", savedEvent.ActorType);
            Assert.Equal(1, savedEvent.SchemaVersion);
            Assert.Contains("john.doe@example.com", savedEvent.EntityData);
            _schemaValidatorMock.Verify(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TrackEventAsync_InvalidData_ThrowsException()
        {
            // Arrange
            var invalidCustomer = new Customer("john doe", "wrong-email-format");
            _schemaValidatorMock
                .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException("Entity data does not match schema"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _eventTracker.TrackEventAsync(
                    entityType: "Customer",
                    entityId: "1",
                    eventType: "CustomerCreated",
                    actorId: "test-user",
                    actorType: "User",
                    entityData: invalidCustomer));

            // Verify no events were saved
            Assert.Empty(await _context.BusinessEvents.ToListAsync());
            _schemaValidatorMock.Verify(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TrackEventAsync_NonExistentSchema_ThrowsException()
        {
            // Arrange
            var vehicle = new { Id = 1, Make = "Toyota", Model = "Corolla" };
            // No need to setup schema validator, as schema will not be found

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _eventTracker.TrackEventAsync(
                    entityType: "Vehicle",
                    entityId: "1",
                    eventType: "VehicleCreated",
                    actorId: "test-user",
                    actorType: "User",
                    entityData: vehicle));

            // Verify no events were saved
            Assert.Empty(await _context.BusinessEvents.ToListAsync());
            _schemaValidatorMock.Verify(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAllEventsAsync_ReturnsAllEvents()
        {
            // Arrange
            Customer customer1 = new Customer("Alice", "alice@example.com");
            Customer customer2 = new Customer("Bob", "bob@example.com");
            _schemaValidatorMock.Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()));

            await _eventTracker.TrackEventAsync(
                entityType: "Customer",
                entityId: "1",
                eventType: "CustomerCreated",
                actorId: "actor1",
                actorType: "User",
                entityData: customer1);

            await _eventTracker.TrackEventAsync(
                entityType: "Customer",
                entityId: "2",
                eventType: "CustomerCreated",
                actorId: "actor2",
                actorType: "User",
                entityData: customer2);

            // Act
            var events = await _eventTracker.GetAllEventsAsync();

            // Assert
            Assert.NotNull(events);
            Assert.Equal(2, events.Count);
            Assert.Contains(events, e => e.EntityId == "1" && e.ActorId == "actor1");
            Assert.Contains(events, e => e.EntityId == "2" && e.ActorId == "actor2");
        }

        [Fact]
        public async Task GetAllEventsAsync_ReturnsEmptyListWhenNoEvents()
        {
            // Act
            var events = await _eventTracker.GetAllEventsAsync();

            // Assert
            Assert.NotNull(events);
            Assert.Empty(events);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
