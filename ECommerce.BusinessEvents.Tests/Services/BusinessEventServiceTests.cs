using ECommerce.BusinessEvents.Services;
using ECommerce.Contracts.Interfaces;
using Moq;
using Microsoft.Extensions.Logging;

namespace ECommerce.BusinessEvents.Tests.Services
{
    public class BusinessEventServiceTests
    {
        [Fact]
        public async Task TrackEventAsync_DelegatesToEventTrackingService()
        {
            // Arrange
            var mockEventTrackingService = new Mock<IEventTrackingService>();
            mockEventTrackingService
                .Setup(s => s.TrackEventAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var loggerMock = new Mock<ILogger<BusinessEventService>>();
            var service = new BusinessEventService(mockEventTrackingService.Object, loggerMock.Object);

            // Act
            await service.TrackEventAsync(
                "EntityType",
                "123",
                IBusinessEventService.EventType.Created,
                "actorId",
                IBusinessEventService.ActorType.Admin,
                new { Foo = "Bar" });

            // Assert
            mockEventTrackingService.Verify(s => s.TrackEventAsync(
                "EntityType",
                "123",
                "Created",
                "actorId",
                "Admin",
                It.IsAny<object>()), Times.Once);
        }
    }
}
