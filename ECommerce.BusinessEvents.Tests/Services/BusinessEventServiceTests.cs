using ECommerce.BusinessEvents.Services;
using Moq;

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
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = new BusinessEventService(mockEventTrackingService.Object);

            // Act
            await service.TrackEventAsync(
                "EntityType",
                123,
                "EventType",
                "actorId",
                "actorType",
                new { Foo = "Bar" });

            // Assert
            mockEventTrackingService.Verify(s => s.TrackEventAsync(
                "EntityType",
                123,
                "EventType",
                "actorId",
                "actorType",
                It.IsAny<object>()), Times.Once);
        }
    }
}
