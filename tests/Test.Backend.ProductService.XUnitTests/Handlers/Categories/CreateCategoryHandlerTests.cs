using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Category.Request;
using Test.Backend.Abstractions.Models.Dto.Category.Response;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Category;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Handlers.Categories;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.ProductService.XUnitTests.Handlers.Categories
{
    public class CreateCategoryHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<CreateCategoryStartedHandler>> _loggerMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly CreateCategoryStartedHandler _handler;

        public CreateCategoryHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<CreateCategoryStartedHandler>>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _kafkaOptionsMock.Setup(o => o.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });

            _handler = new CreateCategoryStartedHandler(
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
            var createCategoryEvent = new CreateCategoryStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new CategoryRequest { Id = Guid.NewGuid(), Name = "New Category" },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var category = new Category { Id = createCategoryEvent.Activity.Id, Name = createCategoryEvent.Activity.Name };
            var categoryBaseDto = new CategoryBaseDto { Id = category.Id, Name = category.Name };

            _mapperMock.Setup(m => m.Map<Category>(createCategoryEvent.Activity)).Returns(category);
            _mapperMock.Setup(m => m.Map<CategoryBaseDto>(category)).Returns(categoryBaseDto);
            _categoryServiceMock.Setup(s => s.SaveAsync(category)).Returns(Task.CompletedTask);

            // Act
            await _handler.HandleAsync(createCategoryEvent);

            // Assert
            _categoryServiceMock.Verify(s => s.SaveAsync(category), Times.Once);
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<CreateCategoryResponse>(r => r.IsSuccess && r.Dto == categoryBaseDto),
                _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                createCategoryEvent.CorrelationId,
                null
            ), Times.Once);
        }
    }
}
