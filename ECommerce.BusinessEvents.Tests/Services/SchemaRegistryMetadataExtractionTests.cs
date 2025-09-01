using ECommerce.BusinessEvents.Services;
using ECommerce.BusinessEvents.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.BusinessEvents.Tests.Services
{
    public class SchemaRegistryMetadataExtractionTests
    {
        private BusinessEventDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<BusinessEventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new BusinessEventDbContext(options);
        }

        private SchemaRegistryService CreateService(BusinessEventDbContext context)
        {
            var loggerMock = new Mock<ILogger<SchemaRegistryService>>();
            return new SchemaRegistryService(context, loggerMock.Object);
        }

        [Fact]
        public void ParseMetadataConfig_CustomerSchema_ExtractsCorrectFields()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = CreateService(context);

            var customerSchema = @"{
                ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
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
                        ""type"": ""string""
                    },
                    ""CreatedAt"": {
                        ""type"": ""string"",
                        ""format"": ""date-time"",
                        ""x-metadata"": true
                    }
                }
            }";

            // Act
            var config = service.ParseMetadataConfig(customerSchema);

            // Assert
            Assert.True(config.HasMetadata);
            Assert.Equal(4, config.FieldsToExtract.Count);
            Assert.Contains("Id", config.FieldsToExtract);
            Assert.Contains("Name", config.FieldsToExtract);
            Assert.Contains("Email", config.FieldsToExtract);
            Assert.Contains("CreatedAt", config.FieldsToExtract);
            Assert.DoesNotContain("Phone", config.FieldsToExtract);

            // Verify data types
            Assert.Equal("string", config.FieldTypes["Id"]);
            Assert.Equal("string", config.FieldTypes["Name"]);
            Assert.Equal("string", config.FieldTypes["Email"]);
            Assert.Equal("date", config.FieldTypes["CreatedAt"]);
        }

        [Fact]
        public void ParseMetadataConfig_ProductSchema_ExtractsCorrectFields()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = CreateService(context);

            var productSchema = @"{
                ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
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
                    ""Price"": {
                        ""type"": ""number"",
                        ""x-metadata"": true
                    }
                }
            }";

            // Act
            var config = service.ParseMetadataConfig(productSchema);

            // Assert
            Assert.True(config.HasMetadata);
            Assert.Equal(3, config.FieldsToExtract.Count);
            Assert.Contains("Id", config.FieldsToExtract);
            Assert.Contains("Name", config.FieldsToExtract);
            Assert.Contains("Price", config.FieldsToExtract);

            // Verify data types
            Assert.Equal("string", config.FieldTypes["Id"]);
            Assert.Equal("string", config.FieldTypes["Name"]);
            Assert.Equal("number", config.FieldTypes["Price"]);
        }

        [Fact]
        public void ParseMetadataConfig_NestedObjectSchema_ExtractsWithDotNotation()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = CreateService(context);

            var schemaWithNesting = @"{
                ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
                ""type"": ""object"",
                ""properties"": {
                    ""Id"": {
                        ""type"": ""string"",
                        ""x-metadata"": true
                    },
                    ""Address"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""PostCode"": {
                                ""type"": ""string"",
                                ""x-metadata"": true
                            },
                            ""Street"": {
                                ""type"": ""string""
                            }
                        }
                    }
                }
            }";

            // Act
            var config = service.ParseMetadataConfig(schemaWithNesting);

            // Assert
            Assert.True(config.HasMetadata);
            Assert.Equal(2, config.FieldsToExtract.Count);
            Assert.Contains("Id", config.FieldsToExtract);
            Assert.Contains("Address.PostCode", config.FieldsToExtract);
            Assert.DoesNotContain("Address.Street", config.FieldsToExtract);

            // Verify data types
            Assert.Equal("string", config.FieldTypes["Id"]);
            Assert.Equal("string", config.FieldTypes["Address.PostCode"]);
        }

        [Fact]
        public void ParseMetadataConfig_NoMetadataAnnotations_ReturnsEmptyConfig()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = CreateService(context);

            var schemaWithoutMetadata = @"{
                ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
                ""type"": ""object"",
                ""properties"": {
                    ""Id"": {
                        ""type"": ""string""
                    },
                    ""Name"": {
                        ""type"": ""string""
                    }
                }
            }";

            // Act
            var config = service.ParseMetadataConfig(schemaWithoutMetadata);

            // Assert
            Assert.False(config.HasMetadata);
            Assert.Empty(config.FieldsToExtract);
            Assert.Empty(config.FieldTypes);
        }

        [Fact]
        public void ParseMetadataConfig_InvalidJson_ReturnsEmptyConfig()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = CreateService(context);

            var invalidSchema = "{ invalid json";

            // Act
            var config = service.ParseMetadataConfig(invalidSchema);

            // Assert
            Assert.False(config.HasMetadata);
            Assert.Empty(config.FieldsToExtract);
            Assert.Empty(config.FieldTypes);
        }
    }
}
