using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Request;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.WebApi.Controllers.v1;

namespace Test.Backend.WebApi.XUnitTests.Controller
{
    public class OrderControllerTests
    {
        private readonly Mock<IEventBusService> _mockEventBusService;
        private readonly Mock<IOptions<KafkaOptions>> _mockKafkaOptions;
        private readonly Mock<ILogger<OrderController>> _mockLogger;
        private readonly KafkaOptions _kafkaOptions;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _mockEventBusService = new Mock<IEventBusService>();
            _mockKafkaOptions = new Mock<IOptions<KafkaOptions>>();
            _mockLogger = new Mock<ILogger<OrderController>>();

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

            _controller = new OrderController(_mockEventBusService.Object, _mockKafkaOptions.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateOrder_Successful()
        {
            // Arrange
            var orderRequest = new OrderRequest
            {
                UserId = Guid.NewGuid(),
                DeliveryAddressId = Guid.NewGuid(),
                ProductIds = new List<Guid> { Guid.NewGuid() },
                OrderDate = DateTime.UtcNow
            };

            var orderDto = new OrderDto
            {
                Address = new() { Id = Guid.NewGuid(), City = "Turin" },
                Id= Guid.NewGuid(),
                OrderDate= DateTime.UtcNow,
                User = new() { Id = Guid.NewGuid(), FirstName = "Matt" },
                Products = new List<ProductDto>()
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Category = new()
                        {
                            Id= Guid.NewGuid(),
                            Name= "Category"
                        },
                        Name = "Print",
                        Price = 22
                    }
                }
            };

            var response = new CreateOrderResponse { IsSuccess = true, Dto = orderDto };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<CreateOrderStartedEvent, OrderRequest, CreateOrderResponse>(
                    It.IsAny<OrderRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateOrder(orderRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response.Dto, okResult.Value);
        }

        [Fact]
        public async Task CreateOrder_Fails()
        {
            // Arrange
            var orderRequest = new OrderRequest
            {
                UserId = Guid.NewGuid(),
                DeliveryAddressId = Guid.NewGuid(),
                ProductIds = new List<Guid> { Guid.NewGuid() },
                OrderDate = DateTime.UtcNow
            };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<CreateOrderStartedEvent, OrderRequest, CreateOrderResponse>(
                    It.IsAny<OrderRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateOrderResponse)null);

            // Act
            var result = await _controller.CreateOrder(orderRequest, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetOrders_Successful()
        {
            // Arrange
            var response = new GetOrdersResponse
            {
                IsSuccess = true,
                Dto = new List<OrderDto> { new OrderDto() }
            };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetOrdersStartedEvent, object, GetOrdersResponse>(
                    It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetOrders(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response.Dto, okResult.Value);
        }

        [Fact]
        public async Task GetOrders_Fails()
        {
            // Arrange
            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetOrdersStartedEvent, object, GetOrdersResponse>(
                    It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetOrdersResponse)null);

            // Act
            var result = await _controller.GetOrders(CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetOrderById_Successful()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var response = new GetOrderResponse
            {
                IsSuccess = true,
                Dto = new OrderDto { Id = orderId }
            };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetOrderStartedEvent, OrderRequest, GetOrderResponse>(
                    It.IsAny<OrderRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetOrderById(orderId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response.Dto, okResult.Value);
        }

        [Fact]
        public async Task GetOrderById_Fails()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetOrderStartedEvent, OrderRequest, GetOrderResponse>(
                    It.IsAny<OrderRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetOrderResponse)null);

            // Act
            var result = await _controller.GetOrderById(orderId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_Successful()
        {
            // Arrange
            var orderRequest = new OrderRequest
            {
                UserId = Guid.NewGuid(),
                DeliveryAddressId = Guid.NewGuid(),
                ProductIds = new List<Guid> { Guid.NewGuid() },
                OrderDate = DateTime.UtcNow
            };

            var orderDto = new OrderDto
            {
                Address = new() { Id = Guid.NewGuid(), City = "Turin" },
                Id = Guid.NewGuid(),
                OrderDate = DateTime.UtcNow,
                User = new() { Id = Guid.NewGuid(), FirstName = "Matt" },
                Products = new List<ProductDto>()
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Category = new()
                        {
                            Id= Guid.NewGuid(),
                            Name= "Category"
                        },
                        Name = "Print",
                        Price = 22
                    }
                }
            };

            var response = new UpdateOrderResponse { IsSuccess = true, Dto = orderDto };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<UpdateOrderStartedEvent, OrderRequest, UpdateOrderResponse>(
                    It.IsAny<OrderRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateUser(orderRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response.Dto, okResult.Value);
        }

        [Fact]
        public async Task UpdateOrder_Fails()
        {
            // Arrange
            var orderRequest = new OrderRequest
            {
                UserId = Guid.NewGuid(),
                DeliveryAddressId = Guid.NewGuid(),
                ProductIds = new List<Guid> { Guid.NewGuid() },
                OrderDate = DateTime.UtcNow
            };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<UpdateOrderStartedEvent, OrderRequest, UpdateOrderResponse>(
                    It.IsAny<OrderRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateOrderResponse)null);

            // Act
            var result = await _controller.UpdateUser(orderRequest, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteOrder_Successful()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var response = new DeleteOrderResponse { IsSuccess = true };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<DeleteOrderStartedEvent, OrderRequest, DeleteOrderResponse>(
                    It.IsAny<OrderRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteUser(orderId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteOrder_Fails()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<DeleteOrderStartedEvent, OrderRequest, DeleteOrderResponse>(
                    It.IsAny<OrderRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeleteOrderResponse)null);

            // Act
            var result = await _controller.DeleteUser(orderId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

    }
}
