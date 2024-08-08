using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Category.Response;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Category;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Handlers.Categories;
using Test.Backend.Services.ProductService.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Category.Request;

namespace Test.Backend.ProductService.XUnitTests.Handlers.Categories
{
    public class UpdateCategoryHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<UpdateCategoryStartedHandler>> _loggerMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly UpdateCategoryStartedHandler _handler;

        public UpdateCategoryHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<UpdateCategoryStartedHandler>>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _kafkaOptionsMock.Setup(o => o.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });

            _handler = new UpdateCategoryStartedHandler(
                _msgBusMock.Object,
                _categoryServiceMock.Object,
                _mapperMock.Object,
                _kafkaOptionsMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task HandleAsync_Successful()
        {
            // Arrange
            var updateCategoryEvent = new UpdateCategoryStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new CategoryRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "Updated Category"
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var existingCategory = new Category
            {
                Id = updateCategoryEvent.Activity.Id,
                Name = "Old Category"
            };

            var updatedCategory = new Category
            {
                Id = updateCategoryEvent.Activity.Id,
                Name = "Updated Category"
            };

            var updatedCategoryDto = new CategoryBaseDto
            {
                Id = updateCategoryEvent.Activity.Id,
                Name = "Updated Category"
            };

            _categoryServiceMock.Setup(s => s.GetByIdAsync(updateCategoryEvent.Activity.Id)).ReturnsAsync(existingCategory);
            _mapperMock.Setup(m => m.Map<Category>(updateCategoryEvent.Activity)).Returns(updatedCategory);
            _categoryServiceMock.Setup(s => s.UpdateAsync(updatedCategory)).Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<CategoryBaseDto>(updatedCategory)).Returns(updatedCategoryDto);

            // Act
            await _handler.HandleAsync(updateCategoryEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<UpdateCategoryResponse>(r => r.IsSuccess && r.Dto == updatedCategoryDto),
                _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                updateCategoryEvent.CorrelationId,
                null
            ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var updateCategoryEvent = new UpdateCategoryStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new CategoryRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "Updated Category"
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            _categoryServiceMock.Setup(s => s.GetByIdAsync(updateCategoryEvent.Activity.Id)).ReturnsAsync((Category?)null);

            // Act
            await _handler.HandleAsync(updateCategoryEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<UpdateCategoryResponse>(r => !r.IsSuccess && r.Dto == null),
                _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                updateCategoryEvent.CorrelationId,
                null
            ), Times.Once);
        }
    }
}
