using Microsoft.EntityFrameworkCore;
using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Services;
using ECommerce.Modules.Customers.Domain;
using ECommerce.BusinessEvents.Infrastructure.Validators;
using Moq;
using ECommerce.Contracts.DTOs;
using ECommerce.Contracts.Interfaces;

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
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EventTrackingService>>();
            _eventTracker = new EventTrackingService(_context, _schemaRegistry, _schemaValidatorMock.Object, loggerMock.Object);

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
                .Returns(ECommerce.Common.Result<ECommerce.Common.Unit, string>.Success(new ECommerce.Common.Unit()));

            var dto = new BusinessEventDto
            {
                EntityType = "Customer",
                EntityId = "1",
                EventType = IBusinessEventService.EventType.Created,
                SchemaVersion = 1,
                EventTimestamp = DateTimeOffset.UtcNow,
                CorrelationId = Guid.NewGuid().ToString(),
                ActorId = "test-user",
                ActorType = IBusinessEventService.ActorType.User,
                EntityData = customer
            };

            // Act
            var result = await _eventTracker.TrackEventAsync(dto);
            Assert.True(result.IsSuccess, result.Error);

            // Assert
            var savedEvent = await _context.BusinessEvents.FirstOrDefaultAsync();
            Assert.NotNull(savedEvent);
            Assert.Equal("Customer", savedEvent.EntityType);
            Assert.Equal("1", savedEvent.EntityId);
            Assert.Equal("Created", savedEvent.EventType);
            Assert.Equal("test-user", savedEvent.ActorId);
            Assert.Equal("User", savedEvent.ActorType);
            Assert.Equal(1, savedEvent.SchemaVersion);
            Assert.Contains("john.doe@example.com", savedEvent.EntityData);
            _schemaValidatorMock.Verify(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TrackEventAsync_InvalidData_ReturnsFailure()
        {
            // Arrange
            var invalidCustomer = new Customer("john doe", "wrong-email-format");
            _schemaValidatorMock
                .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ECommerce.Common.Result<ECommerce.Common.Unit, string>.Failure("Entity data does not match schema"));

            var dto = new BusinessEventDto
            {
                EntityType = "Customer",
                EntityId = "1",
                EventType = IBusinessEventService.EventType.Created,
                SchemaVersion = 1,
                EventTimestamp = DateTimeOffset.UtcNow,
                CorrelationId = Guid.NewGuid().ToString(),
                ActorId = "test-user",
                ActorType = IBusinessEventService.ActorType.User,
                EntityData = invalidCustomer
            };

            // Act
            var result = await _eventTracker.TrackEventAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Entity data does not match schema", result.Error);
            Assert.Empty(await _context.BusinessEvents.ToListAsync());
            _schemaValidatorMock.Verify(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TrackEventAsync_NonExistentSchema_ReturnsFailure()
        {
            // Arrange
            var vehicle = new { Id = 1, Make = "Toyota", Model = "Corolla" };
            var dto = new BusinessEventDto
            {
                EntityType = "Vehicle",
                EntityId = "1",
                EventType = IBusinessEventService.EventType.Created,
                SchemaVersion = 1,
                EventTimestamp = DateTimeOffset.UtcNow,
                CorrelationId = Guid.NewGuid().ToString(),
                ActorId = "test-user",
                ActorType = IBusinessEventService.ActorType.User,
                EntityData = vehicle
            };

            // Act
            var result = await _eventTracker.TrackEventAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("No schema found", result.Error);
            Assert.Empty(await _context.BusinessEvents.ToListAsync());
            _schemaValidatorMock.Verify(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAllEventsAsync_ReturnsAllEvents()
        {
            // Arrange
            Customer customer1 = new Customer("Alice", "alice@example.com");
            Customer customer2 = new Customer("Bob", "bob@example.com");
            _schemaValidatorMock.Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ECommerce.Common.Result<ECommerce.Common.Unit, string>.Success(new ECommerce.Common.Unit()));

            var dto1 = new BusinessEventDto
            {
                EntityType = "Customer",
                EntityId = "1",
                EventType = IBusinessEventService.EventType.Created,
                SchemaVersion = 1,
                EventTimestamp = DateTimeOffset.UtcNow,
                CorrelationId = Guid.NewGuid().ToString(),
                ActorId = "actor1",
                ActorType = IBusinessEventService.ActorType.User,
                EntityData = customer1
            };

            var dto2 = new BusinessEventDto
            {
                EntityType = "Customer",
                EntityId = "2",
                EventType = IBusinessEventService.EventType.Created,
                SchemaVersion = 1,
                EventTimestamp = DateTimeOffset.UtcNow,
                CorrelationId = Guid.NewGuid().ToString(),
                ActorId = "actor2",
                ActorType = IBusinessEventService.ActorType.User,
                EntityData = customer2
            };

            var result1 = await _eventTracker.TrackEventAsync(dto1);
            Assert.True(result1.IsSuccess, result1.Error);
            var result2 = await _eventTracker.TrackEventAsync(dto2);
            Assert.True(result2.IsSuccess, result2.Error);

            // Act
            var events = await _eventTracker.GetAllEventsAsync();

            // Assert
            Assert.NotNull(events);
            Assert.Equal(2, events.Count);
            Assert.Contains(events, e => e is { EntityId: "1", ActorId: "actor1" });
            Assert.Contains(events, e => e is { EntityId: "2", ActorId: "actor2" });
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
