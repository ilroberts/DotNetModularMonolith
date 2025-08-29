using ECommerce.BusinessEvents.Services;
using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace ECommerce.BusinessEvents.Tests.Services
{
    public class EventQueryServiceTests
    {
        private BusinessEventDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<BusinessEventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new BusinessEventDbContext(options);
        }

        private Mock<ILogger<EventQueryService>> CreateMockLogger()
        {
            return new Mock<ILogger<EventQueryService>>();
        }

        [Fact]
        public async Task GetEntityEventsAsync_WithFieldSelection_ReturnsMetadataFields()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var logger = CreateMockLogger();
            var service = new EventQueryService(context, logger.Object);

            var eventId = Guid.NewGuid();
            var businessEvent = new BusinessEvent
            {
                EventId = eventId,
                EntityType = "Customer",
                EntityId = "123",
                EventType = "Created",
                EventTimestamp = DateTimeOffset.UtcNow,
                ActorId = "user1",
                ActorType = "User",
                EntityData = "{\"Id\":\"123\",\"Name\":\"John Doe\",\"Email\":\"john@example.com\"}",
                SchemaVersion = 1
            };

            var metadata = new List<BusinessEventMetadata>
            {
                new() { EventId = eventId, EntityType = "Customer", EntityId = "123", MetadataKey = "Name", MetadataValue = "John Doe", DataType = "string" },
                new() { EventId = eventId, EntityType = "Customer", EntityId = "123", MetadataKey = "Email", MetadataValue = "john@example.com", DataType = "string" }
            };

            context.BusinessEvents.Add(businessEvent);
            context.BusinessEventMetadata.AddRange(metadata);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetEntityEventsAsync("Customer", "123", new[] { "Name", "Email" });

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            var eventResponse = result.Value.First();
            Assert.Equal(eventId, eventResponse.EventId);
            Assert.Equal("Customer", eventResponse.EntityType);
            Assert.Equal("123", eventResponse.EntityId);
            Assert.Equal(2, eventResponse.Fields.Count);
            Assert.Equal("John Doe", eventResponse.Fields["Name"]);
            Assert.Equal("john@example.com", eventResponse.Fields["Email"]);
            Assert.Null(eventResponse.FullData);
        }

        [Fact]
        public async Task GetEntityEventsAsync_WithoutFieldSelection_ReturnsFullData()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var logger = CreateMockLogger();
            var service = new EventQueryService(context, logger.Object);

            var eventId = Guid.NewGuid();
            var businessEvent = new BusinessEvent
            {
                EventId = eventId,
                EntityType = "Customer",
                EntityId = "123",
                EventType = "Created",
                EventTimestamp = DateTimeOffset.UtcNow,
                ActorId = "user1",
                ActorType = "User",
                EntityData = "{\"Id\":\"123\",\"Name\":\"John Doe\",\"Email\":\"john@example.com\"}",
                SchemaVersion = 1
            };

            context.BusinessEvents.Add(businessEvent);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetEntityEventsAsync("Customer", "123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            var eventResponse = result.Value.First();
            Assert.Equal(eventId, eventResponse.EventId);
            Assert.Equal("{\"Id\":\"123\",\"Name\":\"John Doe\",\"Email\":\"john@example.com\"}", eventResponse.FullData);
            Assert.Empty(eventResponse.Fields);
        }

        [Fact]
        public async Task SearchEventsAsync_WithMetadataFilter_ReturnsFilteredResults()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var logger = CreateMockLogger();
            var service = new EventQueryService(context, logger.Object);

            var event1Id = Guid.NewGuid();
            var event2Id = Guid.NewGuid();

            var events = new List<BusinessEvent>
            {
                new() { EventId = event1Id, EntityType = "Customer", EntityId = "123", EventType = "Created", EventTimestamp = DateTimeOffset.UtcNow, ActorId = "user1", ActorType = "User", EntityData = "{\"Id\":\"123\",\"Name\":\"John Doe\",\"Email\":\"john@example.com\"}", SchemaVersion = 1 },
                new() { EventId = event2Id, EntityType = "Customer", EntityId = "456", EventType = "Updated", EventTimestamp = DateTimeOffset.UtcNow, ActorId = "user1", ActorType = "User", EntityData = "{\"Id\":\"456\",\"Name\":\"Jane Smith\",\"Email\":\"jane@other.com\"}", SchemaVersion = 1 }
            };

            var metadata = new List<BusinessEventMetadata>
            {
                new() { EventId = event1Id, EntityType = "Customer", EntityId = "123", MetadataKey = "Email", MetadataValue = "john@example.com", DataType = "string" },
                new() { EventId = event2Id, EntityType = "Customer", EntityId = "456", MetadataKey = "Email", MetadataValue = "jane@other.com", DataType = "string" }
            };

            context.BusinessEvents.AddRange(events);
            context.BusinessEventMetadata.AddRange(metadata);
            await context.SaveChangesAsync();

            // Act
            var searchRequest = new EventSearchRequest
            {
                EntityType = "Customer",
                Email = "john@example.com"
            };
            var result = await service.SearchEventsAsync(searchRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            var eventResponse = result.Value.First();
            Assert.Equal(event1Id, eventResponse.EventId);
            Assert.Equal("john@example.com", eventResponse.Fields["Email"]);
            // Verify that FullData is now populated with complete JSON
            Assert.Equal("{\"Id\":\"123\",\"Name\":\"John Doe\",\"Email\":\"john@example.com\"}", eventResponse.FullData);
        }

        [Fact]
        public async Task SearchEventsAsync_WithWildcardFilter_ReturnsMatchingResults()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var logger = CreateMockLogger();
            var service = new EventQueryService(context, logger.Object);

            var event1Id = Guid.NewGuid();
            var event2Id = Guid.NewGuid();

            var events = new List<BusinessEvent>
            {
                new() { EventId = event1Id, EntityType = "Customer", EntityId = "123", EventType = "Created", EventTimestamp = DateTimeOffset.UtcNow, ActorId = "user1", ActorType = "User", EntityData = "{\"Id\":\"123\",\"Name\":\"John Doe\",\"Email\":\"john@example.com\"}", SchemaVersion = 1 },
                new() { EventId = event2Id, EntityType = "Customer", EntityId = "456", EventType = "Updated", EventTimestamp = DateTimeOffset.UtcNow, ActorId = "user1", ActorType = "User", EntityData = "{\"Id\":\"456\",\"Name\":\"Jane Smith\",\"Email\":\"jane@example.com\"}", SchemaVersion = 1 }
            };

            var metadata = new List<BusinessEventMetadata>
            {
                new() { EventId = event1Id, EntityType = "Customer", EntityId = "123", MetadataKey = "Email", MetadataValue = "john@example.com", DataType = "string" },
                new() { EventId = event2Id, EntityType = "Customer", EntityId = "456", MetadataKey = "Email", MetadataValue = "jane@example.com", DataType = "string" }
            };

            context.BusinessEvents.AddRange(events);
            context.BusinessEventMetadata.AddRange(metadata);
            await context.SaveChangesAsync();

            // Act
            var searchRequest = new EventSearchRequest
            {
                EntityType = "Customer",
                Email = "*@example.com"
            };
            var result = await service.SearchEventsAsync(searchRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);

            // Verify that both results have FullData populated
            foreach (var eventResponse in result.Value)
            {
                Assert.NotNull(eventResponse.FullData);
                Assert.NotEmpty(eventResponse.FullData);
                Assert.Contains("Email", eventResponse.FullData);
            }
        }
    }
}
