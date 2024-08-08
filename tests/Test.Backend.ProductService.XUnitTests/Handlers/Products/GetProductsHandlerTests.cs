using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.Product.Response;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Product;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Handlers.Products;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.ProductService.XUnitTests.Handlers.Products
{
    public class GetProductsHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<GetProductsStartedHandler>> _loggerMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly GetProductsStartedHandler _handler;

        public GetProductsHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _productServiceMock = new Mock<IProductService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<GetProductsStartedHandler>>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _kafkaOptionsMock.Setup(o => o.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });

            _handler = new GetProductsStartedHandler(
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
            var getProductsEvent = new GetProductsStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new { },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Product1", Price = 10.00, CategoryId = Guid.NewGuid() },
                new Product { Id = Guid.NewGuid(), Name = "Product2", Price = 20.00, CategoryId = Guid.NewGuid() }
            };

            var productsDto = new List<ProductDto>
            {
                new ProductDto { Id = products[0].Id, Name = "Product1", Price = 10.00 },
                new ProductDto { Id = products[1].Id, Name = "Product2", Price = 20.00 }
            };

            _productServiceMock.Setup(s => s.GetAsync()).ReturnsAsync(products);
            _mapperMock.Setup(m => m.Map<List<ProductDto>>(products)).Returns(productsDto);

            // Act
            await _handler.HandleAsync(getProductsEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<GetProductsResponse>(r => r.IsSuccess && r.Dto == productsDto),
                _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                getProductsEvent.CorrelationId,
                null
            ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var getProductsEvent = new GetProductsStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new { },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var products = new List<Product>();

            _productServiceMock.Setup(s => s.GetAsync()).ReturnsAsync(products);

            // Act
            await _handler.HandleAsync(getProductsEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<GetProductsResponse>(r => !r.IsSuccess && r.Dto == null),
                _kafkaOptionsMock.Object!.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                getProductsEvent.CorrelationId,
                null
            ), Times.Once);
        }
    }
}
