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
    public class DeleteCategoryHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<DeleteCategoryStartedHandler>> _loggerMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly DeleteCategoryStartedHandler _handler;

        public DeleteCategoryHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<DeleteCategoryStartedHandler>>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _kafkaOptionsMock.Setup(o => o.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });

            _handler = new DeleteCategoryStartedHandler(
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
            var deleteCategoryEvent = new DeleteCategoryStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new CategoryRequest { Id = Guid.NewGuid(), Name="Delete Category" },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var category = new Category { Id = deleteCategoryEvent.Activity.Id, Name = "Category to Delete" };
            var categoryBaseDto = new CategoryBaseDto { Id = category.Id, Name = category.Name };

            _categoryServiceMock.Setup(s => s.GetByIdAsync(deleteCategoryEvent.Activity.Id)).ReturnsAsync(category);
            _categoryServiceMock.Setup(s => s.DeleteAsync(deleteCategoryEvent.Activity.Id)).ReturnsAsync(true);

            _mapperMock.Setup(m => m.Map<CategoryBaseDto>(category)).Returns(categoryBaseDto);

            // Act
            await _handler.HandleAsync(deleteCategoryEvent);

            // Assert
            _categoryServiceMock.Verify(s => s.GetByIdAsync(deleteCategoryEvent.Activity.Id), Times.Once);
            _categoryServiceMock.Verify(s => s.DeleteAsync(deleteCategoryEvent.Activity.Id), Times.Once);

            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<DeleteCategoryResponse>(r => r.IsSuccess && r.Dto == categoryBaseDto),
                _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                deleteCategoryEvent.CorrelationId,
                null
            ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var deleteCategoryEvent = new DeleteCategoryStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new CategoryRequest { Id = Guid.NewGuid(), Name = "Delete Category" },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var category = new Category { Id = deleteCategoryEvent.Activity.Id, Name = "Category to Delete" };

            _categoryServiceMock.Setup(s => s.GetByIdAsync(deleteCategoryEvent.Activity.Id)).ReturnsAsync(category);
            _categoryServiceMock.Setup(s => s.DeleteAsync(deleteCategoryEvent.Activity.Id)).ReturnsAsync(false);

            // Act
            await _handler.HandleAsync(deleteCategoryEvent);

            // Assert
            _categoryServiceMock.Verify(s => s.GetByIdAsync(deleteCategoryEvent.Activity.Id), Times.Once);
            _categoryServiceMock.Verify(s => s.DeleteAsync(deleteCategoryEvent.Activity.Id), Times.Once);

            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<DeleteCategoryResponse>(r => !r.IsSuccess && r.Dto == null),
                _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                deleteCategoryEvent.CorrelationId,
                null
            ), Times.Once);
        }
    }
}
