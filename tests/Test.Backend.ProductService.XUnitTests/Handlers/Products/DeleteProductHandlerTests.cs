using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Backend.Abstractions.Models.Dto.Product.Request;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Product;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Handlers.Products;
using Test.Backend.Services.ProductService.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Product.Response;

namespace Test.Backend.ProductService.XUnitTests.Handlers.Products
{
    public class DeleteProductHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<DeleteProductStartedHandler>> _loggerMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly DeleteProductStartedHandler _handler;

        public DeleteProductHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _productServiceMock = new Mock<IProductService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<DeleteProductStartedHandler>>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _kafkaOptionsMock.Setup(o => o.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });
            _handler = new DeleteProductStartedHandler(
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
            var deleteProductEvent = new DeleteProductStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new ProductRequest
                {
                    Id = Guid.NewGuid()
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var existingProduct = new Product
            {
                Id = deleteProductEvent.Activity.Id,
                Name = "Product",
                Price = 29.99,
                CategoryId = Guid.NewGuid()
            };

            var productDto = new ProductWithoutOrderDto
            {
                Id = existingProduct.Id,
                Name = existingProduct.Name,
                Price = existingProduct.Price
            };

            _productServiceMock.Setup(s => s.GetByIdAsync(deleteProductEvent.Activity.Id)).ReturnsAsync(existingProduct);
            _productServiceMock.Setup(s => s.DeleteAsync(deleteProductEvent.Activity.Id)).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<ProductWithoutOrderDto>(existingProduct)).Returns(productDto);

            // Act
            await _handler.HandleAsync(deleteProductEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<DeleteProductResponse>(r => r.IsSuccess && r.Dto == productDto),
                _kafkaOptionsMock.Object.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                deleteProductEvent.CorrelationId,
                null
            ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails_Null()
        {
            // Arrange
            var deleteProductEvent = new DeleteProductStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new ProductRequest
                {
                    Id = Guid.NewGuid()
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            _productServiceMock.Setup(s => s.GetByIdAsync(deleteProductEvent.Activity.Id)).ReturnsAsync((Product)null);

            // Act
            await _handler.HandleAsync(deleteProductEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<DeleteProductResponse>(r => !r.IsSuccess && r.Dto == null),
                _kafkaOptionsMock.Object.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                deleteProductEvent.CorrelationId,
                null
            ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var deleteProductEvent = new DeleteProductStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new ProductRequest
                {
                    Id = Guid.NewGuid()
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var existingProduct = new Product
            {
                Id = deleteProductEvent.Activity.Id,
                Name = "Product",
                Price = 29.99,
                CategoryId = Guid.NewGuid()
            };

            _productServiceMock.Setup(s => s.GetByIdAsync(deleteProductEvent.Activity.Id)).ReturnsAsync(existingProduct);
            _productServiceMock.Setup(s => s.DeleteAsync(deleteProductEvent.Activity.Id)).ReturnsAsync(false);

            // Act
            await _handler.HandleAsync(deleteProductEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<DeleteProductResponse>(r => !r.IsSuccess && r.Dto == null),
                _kafkaOptionsMock.Object.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                deleteProductEvent.CorrelationId,
                null
            ), Times.Once);
        }
    }
}
