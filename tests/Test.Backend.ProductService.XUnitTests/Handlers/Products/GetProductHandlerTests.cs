using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Dto.Product.Response;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Product;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Handlers.Products;
using Test.Backend.Services.ProductService.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Product.Request;

namespace Test.Backend.ProductService.XUnitTests.Handlers.Products
{
    public class GetProductHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<GetProductStartedHandler>> _loggerMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly GetProductStartedHandler _handler;

        public GetProductHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _productServiceMock = new Mock<IProductService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<GetProductStartedHandler>>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _kafkaOptionsMock.Setup(o => o.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });
            
            _handler = new GetProductStartedHandler(
                _msgBusMock.Object,
                _productServiceMock.Object,
                _mapperMock.Object,
                _kafkaOptionsMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task HandleAsync_Successful()
        {
            // Arrange
            var getProductEvent = new GetProductStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new ProductRequest
                {
                    Id = Guid.NewGuid()
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var product = new Product
            {
                Id = getProductEvent.Activity.Id,
                Name = "Product",
                Price = 100.00,
                CategoryId = Guid.NewGuid()
            };

            var productDto = new ProductDto
            {
                Id = getProductEvent.Activity.Id,
                Name = "Product",
                Price = 100.00,
                Category = new CategoryBaseDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Category"
                }
            };

            _productServiceMock.Setup(s => s.GetByIdAsync(getProductEvent.Activity.Id)).ReturnsAsync(product);
            _mapperMock.Setup(m => m.Map<ProductDto>(product)).Returns(productDto);

            // Act
            await _handler.HandleAsync(getProductEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<GetProductResponse>(r => r.IsSuccess && r.Dto == productDto),
                 _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                getProductEvent.CorrelationId,
                null
            ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var getProductEvent = new GetProductStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new ProductRequest
                {
                    Id = Guid.NewGuid(),
                    Name= "Product",
                    CategoryId= Guid.NewGuid(),
                    Price = 20
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            _productServiceMock.Setup(s => s.GetByIdAsync(getProductEvent.Activity.Id)).ReturnsAsync((Product?)null);

            // Act
            await _handler.HandleAsync(getProductEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<GetProductResponse>(r => !r.IsSuccess && r.Dto == null),
                 _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                getProductEvent.CorrelationId,
                null
            ), Times.Once);
        }
    }
}
