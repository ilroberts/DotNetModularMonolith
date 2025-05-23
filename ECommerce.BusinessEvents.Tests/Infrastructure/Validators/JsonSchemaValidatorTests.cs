using ECommerce.BusinessEvents.Infrastructure.Validators;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.BusinessEvents.Tests.Infrastructure.Validators
{
    public class JsonSchemaValidatorTests
    {
        private readonly Mock<ILogger<JsonSchemaValidator>> _loggerMock;
        private readonly JsonSchemaValidator _validator;

        public JsonSchemaValidatorTests()
        {
            _loggerMock = new Mock<ILogger<JsonSchemaValidator>>();
            _validator = new JsonSchemaValidator(_loggerMock.Object);
        }

        [Fact]
        public void Validate_ValidJsonAndSchema_DoesNotThrow()
        {
            // Arrange
            var schema = @"{ ""type"": ""object"", ""properties"": { ""name"": { ""type"": ""string"" } }, ""required"": [""name""] }";
            var json = @"{ ""name"": ""Test"" }";

            // Act & Assert
            _validator.Validate(json, schema);
        }

        [Fact]
        public void Validate_InvalidJson_ThrowsAndLogsWarning()
        {
            // Arrange
            var schema = @"{ ""type"": ""object"", ""properties"": { ""name"": { ""type"": ""string"" } }, ""required"": [""name""] }";
            var json = @"{ ""notname"": ""Test"" }";

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _validator.Validate(json, schema));
            Assert.Contains("Validation failed", ex.Message);

            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Schema validation failed")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
