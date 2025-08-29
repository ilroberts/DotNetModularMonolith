using Microsoft.EntityFrameworkCore;
using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Services;
using ECommerce.Modules.Customers.Domain;
using ECommerce.BusinessEvents.Infrastructure.Validators;
using Moq;
using ECommerce.Contracts.DTOs;
using ECommerce.Contracts.Interfaces;
using ECommerce.BusinessEvents.Infrastructure;
using ECommerce.Common;
using Newtonsoft.Json.Linq;
using JsonFlatten;

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
            ITransactionManager transactionManager = new NoOpTransactionManager();
            _eventTracker = new EventTrackingService(_context, _schemaRegistry, _schemaValidatorMock.Object, loggerMock.Object, transactionManager);

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
                .Returns(Result<Unit, string>.Success(new Unit()));

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
                .Returns(Result<Unit, string>.Failure("Entity data does not match schema"));

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
                .Returns(Result<Unit, string>.Success(new Unit()));

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

        [Fact]
        public async Task TrackEventAsync_WithNullAndEmptyFields_OnlySavesMetadataForFieldsWithData()
        {
            // Arrange - Create a schema with metadata extraction configuration
            string customerSchemaWithMetadata = @"{
                ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
                ""$id"": ""https://example.com/schemas/customer-with-metadata/v1"",
                ""title"": ""Customer With Metadata"",
                ""type"": ""object"",
                ""properties"": {
                    ""Id"": {
                        ""type"": ""string"",
                        ""x-metadata"": true
                    },
                    ""Name"": {
                        ""type"": ""string"",
                        ""x-metadata"": true
                    },
                    ""Email"": {
                        ""type"": ""string"",
                        ""format"": ""email"",
                        ""x-metadata"": true
                    },
                    ""Phone"": {
                        ""type"": [""string"", ""null""],
                        ""x-metadata"": true
                    },
                    ""CompanyName"": {
                        ""type"": [""string"", ""null""],
                        ""x-metadata"": true
                    },
                    ""Status"": {
                        ""type"": ""string"",
                        ""x-metadata"": true
                    }
                },
                ""required"": [""Id"", ""Name"", ""Email""]
            }";

            await _schemaRegistry.AddSchemaAsync("CustomerWithMetadata", 1, customerSchemaWithMetadata);

            // Create test data with some null/empty fields
            // Filtering happens in ExtractMetadataFromJson during JSON parsing stage
            var customerData = new
            {
                Id = "test-123",
                Name = "John Doe",
                Email = "john.doe@example.com",
                Phone = (string?)null,           // null field - should be filtered out
                CompanyName = "",                // empty string - should be filtered out
                Status = "Active"                // valid field - should be included
            };

            _schemaValidatorMock
                .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Result<Unit, string>.Success(new Unit()));

            var dto = new BusinessEventDto
            {
                EntityType = "CustomerWithMetadata",
                EntityId = "test-123",
                EventType = IBusinessEventService.EventType.Created,
                SchemaVersion = 1,
                EventTimestamp = DateTimeOffset.UtcNow,
                CorrelationId = Guid.NewGuid().ToString(),
                ActorId = "test-user",
                ActorType = IBusinessEventService.ActorType.User,
                EntityData = customerData
            };

            // Act
            var result = await _eventTracker.TrackEventAsync(dto);

            // Assert
            Assert.True(result.IsSuccess, result.Error);

            // Verify the main event was saved
            var savedEvent = await _context.BusinessEvents.FirstOrDefaultAsync();
            Assert.NotNull(savedEvent);
            Assert.Equal("CustomerWithMetadata", savedEvent.EntityType);
            Assert.Equal("test-123", savedEvent.EntityId);

            // Verify metadata - should only have records for fields with actual data
            // Null and empty fields are filtered out in ExtractMetadataFromJson method
            var metadataRecords = await _context.BusinessEventMetadata
                .Where(m => m.EventId == savedEvent.EventId)
                .ToListAsync();

            // Should have metadata for: Id, Name, Email, Status (4 fields)
            // Should NOT have metadata for: Phone (null), CompanyName (empty string)
            Assert.Equal(4, metadataRecords.Count);

            // Verify specific metadata entries exist with correct values
            var idMetadata = metadataRecords.FirstOrDefault(m => m.MetadataKey == "Id");
            Assert.NotNull(idMetadata);
            Assert.Equal("test-123", idMetadata.MetadataValue);

            var nameMetadata = metadataRecords.FirstOrDefault(m => m.MetadataKey == "Name");
            Assert.NotNull(nameMetadata);
            Assert.Equal("John Doe", nameMetadata.MetadataValue);

            var emailMetadata = metadataRecords.FirstOrDefault(m => m.MetadataKey == "Email");
            Assert.NotNull(emailMetadata);
            Assert.Equal("john.doe@example.com", emailMetadata.MetadataValue);

            var statusMetadata = metadataRecords.FirstOrDefault(m => m.MetadataKey == "Status");
            Assert.NotNull(statusMetadata);
            Assert.Equal("Active", statusMetadata.MetadataValue);

            // Verify null and empty fields are NOT saved as metadata
            var phoneMetadata = metadataRecords.FirstOrDefault(m => m.MetadataKey == "Phone");
            Assert.Null(phoneMetadata);

            var companyMetadata = metadataRecords.FirstOrDefault(m => m.MetadataKey == "CompanyName");
            Assert.Null(companyMetadata);
        }

        [Fact]
        public async Task TrackEventAsync_WithEmbeddedPhoneNumbers_ExtractsMetadataCorrectly()
        {
            // Arrange: Schema with embedded array of phone numbers
            string customerSchemaWithPhones = @"{
                ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
                ""type"": ""object"",
                ""properties"": {
                    ""Id"": { ""type"": ""string"", ""x-metadata"": true },
                    ""Name"": { ""type"": ""string"", ""x-metadata"": true },
                    ""PhoneNumbers"": {
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""object"",
                            ""properties"": {
                                ""Number"": { ""type"": ""string"", ""x-metadata"": true },
                                ""Prefix"": { ""type"": ""string"", ""x-metadata"": true }
                            }
                        }
                    }
                }
            }";

            await _schemaRegistry.AddSchemaAsync("CustomerWithPhones", 1, customerSchemaWithPhones);

            // Debug: Test metadata config parsing directly
            var metadataConfig = _schemaRegistry.ParseMetadataConfig(customerSchemaWithPhones);
            Assert.Equal(2, metadataConfig.FieldsToExtract.Count); // Should have Id and Name
            Assert.Single(metadataConfig.ArrayPathsToExtract); // Should have PhoneNumbers array
            Assert.Contains("Id", metadataConfig.FieldsToExtract);
            Assert.Contains("Name", metadataConfig.FieldsToExtract);
            Assert.Contains("PhoneNumbers", metadataConfig.ArrayPathsToExtract.Keys);

            var customerData = new {
                Id = "cust-001",
                Name = "Ent",
                PhoneNumbers = new[] {
                    new { Number = "123456789", Prefix = "+44" },
                    new { Number = "987654321", Prefix = "+1" }
                }
            };

            _schemaValidatorMock
                .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Result<Unit, string>.Success(new Unit()));

            var dto = new BusinessEventDto
            {
                EntityType = "CustomerWithPhones",
                EntityId = "cust-001",
                EventType = IBusinessEventService.EventType.Created,
                SchemaVersion = 1,
                EventTimestamp = DateTimeOffset.UtcNow,
                CorrelationId = Guid.NewGuid().ToString(),
                ActorId = "test-user",
                ActorType = IBusinessEventService.ActorType.User,
                EntityData = customerData
            };

            // Act
            var result = await _eventTracker.TrackEventAsync(dto);

            // Assert
            Assert.True(result.IsSuccess, result.Error);
            var savedEvent = await _context.BusinessEvents.FirstOrDefaultAsync(e => e.EntityId == "cust-001");
            Assert.NotNull(savedEvent);

            var metadataRecords = await _context.BusinessEventMetadata
                .Where(m => m.EventId == savedEvent.EventId)
                .ToListAsync();

            // Debug: Check what we actually got vs what we expected
            Assert.True(metadataRecords.Count > 0, "No metadata records were extracted at all!");

            // Should have 6 total: Id, Name, PhoneNumbers[0].Number, PhoneNumbers[0].Prefix, PhoneNumbers[1].Number, PhoneNumbers[1].Prefix
            Assert.Equal(6, metadataRecords.Count);

            // Should have Id and Name
            Assert.Contains(metadataRecords, m => m.MetadataKey == "Id" && m.MetadataValue == "cust-001");
            Assert.Contains(metadataRecords, m => m.MetadataKey == "Name" && m.MetadataValue == "Ent");

            // Should have PhoneNumbers[0].Number and PhoneNumbers[0].Prefix
            Assert.Contains(metadataRecords, m => m.MetadataKey == "PhoneNumbers[0].Number" && m.MetadataValue == "123456789");
            Assert.Contains(metadataRecords, m => m.MetadataKey == "PhoneNumbers[0].Prefix" && m.MetadataValue == "+44");

            // Should have PhoneNumbers[1].Number and PhoneNumbers[1].Prefix
            Assert.Contains(metadataRecords, m => m.MetadataKey == "PhoneNumbers[1].Number" && m.MetadataValue == "987654321");
            Assert.Contains(metadataRecords, m => m.MetadataKey == "PhoneNumbers[1].Prefix" && m.MetadataValue == "+1");
        }

        [Fact]
        public void JsonFlatten_BasicTest_ShouldFlattenCorrectly()
        {
            // Arrange
            var testJson = @"{
                ""Id"": ""cust-001"",
                ""Name"": ""Ent"",
                ""PhoneNumbers"": [
                    {""Number"": ""123456789"", ""Prefix"": ""+44""},
                    {""Number"": ""987654321"", ""Prefix"": ""+1""}
                ]
            }";

            // Act
            var jObject = JObject.Parse(testJson);
            var flattened = jObject.Flatten();

            // Assert - Let's see what keys JsonFlatten actually produces
            var keys = flattened.Keys.ToList();
            Assert.Contains("Id", keys);
            Assert.Contains("Name", keys);

            // Check if JsonFlatten produces the expected array keys
            Assert.Contains("PhoneNumbers[0].Number", keys);
            Assert.Contains("PhoneNumbers[0].Prefix", keys);
            Assert.Contains("PhoneNumbers[1].Number", keys);
            Assert.Contains("PhoneNumbers[1].Prefix", keys);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
