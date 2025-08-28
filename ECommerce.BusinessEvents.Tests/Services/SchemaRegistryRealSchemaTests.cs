using ECommerce.BusinessEvents.Services;
using ECommerce.BusinessEvents.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.IO;
using System.Reflection;

namespace ECommerce.BusinessEvents.Tests.Services
{
    public class SchemaRegistryRealSchemaTests
    {
        private BusinessEventDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<BusinessEventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new BusinessEventDbContext(options);
        }

        [Fact]
        public void ParseMetadataConfig_ActualCustomerSchema_ExtractsCorrectFields()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new SchemaRegistryService(context);

            // Load the actual customer schema file
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ECommerce.BusinessEvents.Resources.Schemas.customer.v1.schema.json";

            string customerSchema;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        customerSchema = reader.ReadToEnd();
                    }
                }
                else
                {
                    // Fallback - read from file system
                    var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var schemaPath = Path.Combine(basePath, "..", "..", "..", "..", "ECommerce.BusinessEvents", "Resources", "Schemas", "customer.v1.schema.json");
                    customerSchema = File.ReadAllText(schemaPath);
                }
            }

            // Act
            var config = service.ParseMetadataConfig(customerSchema);

            // Assert
            Assert.True(config.HasMetadata);
            Assert.Contains("Id", config.FieldsToExtract);
            Assert.Contains("Name", config.FieldsToExtract);
            Assert.Contains("Email", config.FieldsToExtract);
            Assert.Contains("CreatedAt", config.FieldsToExtract);
            Assert.Contains("UpdatedAt", config.FieldsToExtract);

            // Phone and DateOfBirth should NOT be extracted (no x-metadata)
            Assert.DoesNotContain("Phone", config.FieldsToExtract);
            Assert.DoesNotContain("DateOfBirth", config.FieldsToExtract);

            // Verify data types
            Assert.Equal("string", config.FieldTypes["Id"]);
            Assert.Equal("string", config.FieldTypes["Name"]);
            Assert.Equal("string", config.FieldTypes["Email"]);
            Assert.Equal("date", config.FieldTypes["CreatedAt"]);
            Assert.Equal("date", config.FieldTypes["UpdatedAt"]);
        }

        [Fact]
        public void ParseMetadataConfig_ActualProductSchema_ExtractsCorrectFields()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new SchemaRegistryService(context);

            // Load the actual product schema file
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ECommerce.BusinessEvents.Resources.Schemas.product.v1.schema.json";

            string productSchema;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        productSchema = reader.ReadToEnd();
                    }
                }
                else
                {
                    // Fallback - read from file system
                    var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var schemaPath = Path.Combine(basePath, "..", "..", "..", "..", "ECommerce.BusinessEvents", "Resources", "Schemas", "product.v1.schema.json");
                    productSchema = File.ReadAllText(schemaPath);
                }
            }

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
    }
}
