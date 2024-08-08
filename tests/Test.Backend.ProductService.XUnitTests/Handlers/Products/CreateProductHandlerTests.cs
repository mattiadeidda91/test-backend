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

namespace Test.Backend.ProductService.XUnitTests.Handlers.Products
{
    public class CreateProductHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<CreateProductStartedHandler>> _loggerMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly CreateProductStartedHandler _handler;

        public CreateProductHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _productServiceMock = new Mock<IProductService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<CreateProductStartedHandler>>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _kafkaOptionsMock.Setup(o => o.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });

            _handler = new CreateProductStartedHandler(
                _msgBusMock.Object,
                _productServiceMock.Object,
                _categoryServiceMock.Object,
                _mapperMock.Object,
                _kafkaOptionsMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task HandleAsync_Successful_CreatesProductAndSendsMessage()
        {
            // Arrange
            var createProductEvent = new CreateProductStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new ProductRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "New Product",
                    Price = 19.99,
                    CategoryId = Guid.NewGuid()
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var product = new Product
            {
                Id = createProductEvent.Activity.Id,
                Name = createProductEvent.Activity.Name,
                Price = createProductEvent.Activity.Price,
                CategoryId = createProductEvent.Activity.CategoryId
            };

            var productDto = new ProductWithoutOrderDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price
            };

            var categoryDb = new Category
            {
                Id = product.CategoryId,
                Name = "Test Category"
            };

            _categoryServiceMock.Setup(s => s.GetByIdAsync(product.CategoryId)).ReturnsAsync(categoryDb);
            _mapperMock.Setup(m => m.Map<Product>(createProductEvent.Activity)).Returns(product);
            _mapperMock.Setup(m => m.Map<ProductWithoutOrderDto>(product)).Returns(productDto);
            _productServiceMock.Setup(s => s.SaveAsync(product)).Returns(Task.CompletedTask);

            // Act
            await _handler.HandleAsync(createProductEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<CreateProductResponse>(r => r.IsSuccess && r.Dto == productDto),
                _kafkaOptionsMock.Object.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                createProductEvent.CorrelationId,
                null
            ), Times.Once);
        }       
    }
}
