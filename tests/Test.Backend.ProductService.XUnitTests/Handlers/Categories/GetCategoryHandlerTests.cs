using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Dto.Category.Request;
using Test.Backend.Abstractions.Models.Dto.Category.Response;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Category;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Handlers.Categories;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.ProductService.XUnitTests.Handlers.Categories
{
    public class GetCategoryHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<GetCategoryStartedHandler>> _loggerMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly GetCategoryStartedHandler _handler;

        public GetCategoryHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<GetCategoryStartedHandler>>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _kafkaOptionsMock.Setup(o => o.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });

            _handler = new GetCategoryStartedHandler(
                _msgBusMock.Object,
                _categoryServiceMock.Object,
                _mapperMock.Object,
                _kafkaOptionsMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task HandleAsync_Saccessful()
        {
            // Arrange
            var getCategoryEvent = new GetCategoryStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new CategoryRequest { Id = Guid.NewGuid() },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var category = new Category
            {
                Id = getCategoryEvent.Activity!.Id,
                Name = "Category"
            };

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };

            _categoryServiceMock.Setup(s => s.GetByIdAsync(getCategoryEvent.Activity.Id)).ReturnsAsync(category);
            _mapperMock.Setup(m => m.Map<CategoryDto>(category)).Returns(categoryDto);

            // Act
            await _handler.HandleAsync(getCategoryEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<GetCategoryResponse>(r => r.IsSuccess && r.Dto == categoryDto),
                _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                getCategoryEvent.CorrelationId,
                null
            ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var getCategoryEvent = new GetCategoryStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new CategoryRequest { Id = Guid.NewGuid() },
                CorrelationId = Guid.NewGuid().ToString()
            };

            _categoryServiceMock.Setup(s => s.GetByIdAsync(getCategoryEvent.Activity.Id)).ReturnsAsync((Category?)null);

            // Act
            await _handler.HandleAsync(getCategoryEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<GetCategoryResponse>(r => !r.IsSuccess && r.Dto == null),
                _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                getCategoryEvent.CorrelationId,
                null
            ), Times.Once);
        }
    }
}
