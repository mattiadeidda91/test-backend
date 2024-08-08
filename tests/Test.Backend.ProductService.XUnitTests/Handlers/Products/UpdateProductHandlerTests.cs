using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Product.Request;
using Test.Backend.Abstractions.Models.Dto.Product.Response;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Product;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Handlers.Products;
using Test.Backend.Services.ProductService.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Category;

namespace Test.Backend.ProductService.XUnitTests.Handlers.Products
{
    public class UpdateProductHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<ILogger<UpdateProductStartedHandler>> _loggerMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly UpdateProductStartedHandler _handler;

        public UpdateProductHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _productServiceMock = new Mock<IProductService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<UpdateProductStartedHandler>>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _kafkaOptionsMock.Setup(o => o.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });
            _handler = new UpdateProductStartedHandler(
                _msgBusMock.Object,
                _productServiceMock.Object,
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
            var updateProductEvent = new UpdateProductStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new ProductRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "Updated Product",
                    Price = 29.99,
                    CategoryId = Guid.NewGuid()
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var existingProduct = new Product
            {
                Id = updateProductEvent.Activity.Id,
                Name = "Old Product",
                Price = 19.99,
                CategoryId = updateProductEvent.Activity.CategoryId
            };

            var updatedProduct = new Product
            {
                Id = updateProductEvent.Activity.Id,
                Name = updateProductEvent.Activity.Name,
                Price = updateProductEvent.Activity.Price,
                CategoryId = updateProductEvent.Activity.CategoryId
            };

            var productDto = new ProductWithoutOrderDto
            {
                Id = updatedProduct.Id,
                Name = updatedProduct.Name,
                Price = updatedProduct.Price
            };

            var categoryDb = new Category
            {
                Id = updatedProduct.CategoryId,
                Name = "Test Category"
            };

            _productServiceMock.Setup(s => s.GetByIdAsync(updateProductEvent.Activity.Id)).ReturnsAsync(existingProduct);
            _categoryServiceMock.Setup(s => s.GetByIdAsync(updatedProduct.CategoryId)).ReturnsAsync(categoryDb);
            _mapperMock.Setup(m => m.Map<Product>(updateProductEvent.Activity)).Returns(updatedProduct);
            _mapperMock.Setup(m => m.Map<ProductWithoutOrderDto>(updatedProduct)).Returns(productDto);
            _productServiceMock.Setup(s => s.UpdateAsync(updatedProduct)).Returns(Task.CompletedTask);

            // Act
            await _handler.HandleAsync(updateProductEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<UpdateProductResponse>(r => r.IsSuccess && r.Dto == productDto),
                _kafkaOptionsMock.Object.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                updateProductEvent.CorrelationId,
                null
            ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var updateProductEvent = new UpdateProductStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new ProductRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "Product",
                    Price = 29.99,
                    CategoryId = Guid.NewGuid()
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            _productServiceMock.Setup(s => s.GetByIdAsync(updateProductEvent.Activity.Id)).ReturnsAsync((Product)null);

            // Act
            await _handler.HandleAsync(updateProductEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<UpdateProductResponse>(r => !r.IsSuccess && r.Dto == null),
                _kafkaOptionsMock.Object.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                updateProductEvent.CorrelationId,
                null
            ), Times.Once);
        }
    }
}
