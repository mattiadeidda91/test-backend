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

namespace Test.Backend.ProductService.XUnitTests.Handlers.Categories
{
    public class GetCategoriesHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<GetCategoriesStartedHandler>> _loggerMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly GetCategoriesStartedHandler _handler;

        public GetCategoriesHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<GetCategoriesStartedHandler>>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _kafkaOptionsMock.Setup(o => o.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });

            _handler = new GetCategoriesStartedHandler(
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
            var getCategoriesEvent = new GetCategoriesStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new 
                { 
                    
                }, 
                CorrelationId = Guid.NewGuid().ToString()
            };

            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Category 1" },
                new Category { Id = Guid.NewGuid(), Name = "Category 2" }
            };

            var categoryDtos = new List<CategoryDto>
            {
                new CategoryDto { Id = categories[0].Id, Name = categories[0].Name },
                new CategoryDto { Id = categories[1].Id, Name = categories[1].Name }
            };

            _categoryServiceMock.Setup(s => s.GetAsync()).ReturnsAsync(categories);
            _mapperMock.Setup(m => m.Map<List<CategoryDto>>(categories)).Returns(categoryDtos);

            // Act
            await _handler.HandleAsync(getCategoriesEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<GetCategoriesResponse>(r => r.IsSuccess && r.Dto == categoryDtos),
                _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                getCategoriesEvent.CorrelationId,
                null
            ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var getCategoriesEvent = new GetCategoriesStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new {  },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var categories = new List<Category>();

            _categoryServiceMock.Setup(s => s.GetAsync()).ReturnsAsync(categories);

            // Act
            await _handler.HandleAsync(getCategoriesEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<GetCategoriesResponse>(r => !r.IsSuccess && r.Dto == null),
                _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                getCategoriesEvent.CorrelationId,
                null
            ), Times.Once);
        }
    }
}
