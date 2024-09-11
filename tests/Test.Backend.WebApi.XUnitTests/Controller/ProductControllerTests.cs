using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.Product.Request;
using Test.Backend.Abstractions.Models.Dto.Product.Response;
using Test.Backend.Abstractions.Models.Events.Product;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.WebApi.Controllers.v1;

namespace Test.Backend.WebApi.XUnitTests.Controller
{
    public class ProductControllerTests
    {
        private readonly Mock<IEventBusService> _mockEventBusService;
        private readonly Mock<IOptions<KafkaOptions>> _mockKafkaOptions;
        private readonly Mock<ILogger<ProductController>> _mockLogger;
        private readonly KafkaOptions _kafkaOptions;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mockEventBusService = new Mock<IEventBusService>();
            _mockKafkaOptions = new Mock<IOptions<KafkaOptions>>();
            _mockLogger = new Mock<ILogger<ProductController>>();

            _kafkaOptions = new KafkaOptions
            {
                Consumers = new Consumers
                {
                    AddressTopic = "product-topic"
                },
                Producers = new Producers
                {
                    ConsumerTopic = "consumer-topic"
                }
            };

            _mockKafkaOptions.Setup(opt => opt.Value).Returns(_kafkaOptions);

            _controller = new ProductController(_mockEventBusService.Object, _mockKafkaOptions.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateProduct_Successful()
        {
            // Arrange
            var productRequest = new ProductRequest { Name = "Photo", Price = 100 };
            var productDto = new ProductWithoutOrderDto { Name = "Photo", Price = 100 };
            var response = new CreateProductResponse { IsSuccess = true, Dto = productDto };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<CreateProductStartedEvent, ProductRequest, CreateProductResponse>(
                    It.IsAny<ProductRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateProduct(productRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task CreateProduct_Fails()
        {
            // Arrange
            var productRequest = new ProductRequest { Name = "Photo", Price = 100 };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<CreateProductStartedEvent, ProductRequest, CreateProductResponse>(
                    It.IsAny<ProductRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateProductResponse)null);

            // Act
            var result = await _controller.CreateProduct(productRequest, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetProducts_Successful()
        {
            // Arrange
            var response = new GetProductsResponse { IsSuccess = true, Dto = new List<ProductDto> { new ProductDto() } };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetProductsStartedEvent, object, GetProductsResponse>(
                    It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetProducts(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetProducts_Fails()
        {
            // Arrange
            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetProductsStartedEvent, object, GetProductsResponse>(
                    It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetProductsResponse)null);

            // Act
            var result = await _controller.GetProducts(CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetProductById_Successful()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var response = new GetProductResponse { IsSuccess = true, Dto = new ProductDto { Id = productId } };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetProductStartedEvent, ProductRequest, GetProductResponse>(
                    It.IsAny<ProductRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetProductById(productId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetProductById_Fails()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetProductStartedEvent, ProductRequest, GetProductResponse>(
                    It.IsAny<ProductRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetProductResponse)null);

            // Act
            var result = await _controller.GetProductById(productId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_Successful()
        {
            // Arrange
            var productRequest = new ProductRequest { Name = "Updated Photo", Price = 120 };
            var productDto = new ProductWithoutOrderDto { Name = "Updated Photo", Price = 120 };
            var response = new UpdateProductResponse { IsSuccess = true, Dto = productDto };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<UpdateProductStartedEvent, ProductRequest, UpdateProductResponse>(
                    It.IsAny<ProductRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateUser(productRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task UpdateProduct_Fails()
        {
            // Arrange
            var productRequest = new ProductRequest { Name = "Updated Photo", Price = 120 };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<UpdateProductStartedEvent, ProductRequest, UpdateProductResponse>(
                    It.IsAny<ProductRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateProductResponse)null);

            // Act
            var result = await _controller.UpdateUser(productRequest, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_Successful()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var response = new DeleteProductResponse { IsSuccess = true };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<DeleteProductStartedEvent, ProductRequest, DeleteProductResponse>(
                    It.IsAny<ProductRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteUser(productId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_Fails()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<DeleteProductStartedEvent, ProductRequest, DeleteProductResponse>(
                    It.IsAny<ProductRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeleteProductResponse)null);

            // Act
            var result = await _controller.DeleteUser(productId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, notFoundResult.StatusCode);
        }
    }
}
