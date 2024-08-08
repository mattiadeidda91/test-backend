using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.OrderService.Handlers;
using Test.Backend.Services.OrderService.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Order.Request;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Dto.Product;

namespace Test.Backend.OrderService.XUnitTests.Handlers
{
    public class CreateOrderHandlerTests
    {
        private readonly Mock<IEventBusService> _mockMsgBus;
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<IOrderEntitesService> _orderEntitesServiceMock;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<CreateOrderStartedHandler>> _mockLogger;
        private readonly IOptions<KafkaOptions> _optionsKafka;
        private readonly CreateOrderStartedHandler _handler;

        public CreateOrderHandlerTests()
        {
            _mockMsgBus = new Mock<IEventBusService>();
            _mockOrderService = new Mock<IOrderService>();
            _orderEntitesServiceMock = new Mock<IOrderEntitesService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<CreateOrderStartedHandler>>();
            _optionsKafka = Options.Create(new KafkaOptions { Producers = new Producers { ConsumerTopic = "response-topic" } });

            _handler = new CreateOrderStartedHandler(
                _mockMsgBus.Object,
                _mockOrderService.Object,
                _orderEntitesServiceMock.Object,
                _mockMapper.Object,
                _optionsKafka,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task HandleAsync_Successful()
        {
            // Arrange
            var @event = new CreateOrderStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new OrderRequest { Id = Guid.NewGuid() },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderProducts = new List<OrderProduct>
                {
                    new OrderProduct { OrderId = Guid.NewGuid(), ProductId = Guid.NewGuid() }
                }
            };

            var orderDto = new OrderDto
            {
                Id = Guid.NewGuid(),
                User = new UserDto { Id = Guid.NewGuid(), FirstName = "Test User" },
                Address = new AddressDto { Id = Guid.NewGuid(), Street = "Test Street" },
                Products = new List<ProductDto>
                {
                    new ProductDto { Id = Guid.NewGuid(), Name = "Product 1" },
                    new ProductDto { Id = Guid.NewGuid(), Name = "Product 2" }
                }
            };

            _mockMapper.Setup(m => m.Map<Order>(@event.Activity)).Returns(order);
            _orderEntitesServiceMock.Setup(s => s.CheckAndGetExistingEntities(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<List<Guid>>()
            )).ReturnsAsync((true, new UserDto(), new AddressDto(), orderDto.Products.ToList()));
            _mockOrderService.Setup(s => s.SaveAsync(order)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<OrderDto>(order)).Returns(orderDto);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _mockOrderService.Verify(s => s.SaveAsync(order), Times.Once);
            _mockMsgBus.Verify(m =>
                m.SendMessage(It.Is<CreateOrderResponse>(r => r.IsSuccess && r.Dto == orderDto),
                _optionsKafka.Value!.Producers!.ConsumerTopic!, It.IsAny<CancellationToken>(),
                @event.CorrelationId, null),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var @event = new CreateOrderStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new OrderRequest { Id = Guid.NewGuid() },
                CorrelationId = Guid.NewGuid().ToString()
            };

            _mockMapper.Setup(m => m.Map<Order>(@event.Activity)).Returns((Order)null);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _mockOrderService.Verify(s => s.SaveAsync(It.IsAny<Order>()), Times.Never);
            _mockMsgBus.Verify(m => m.SendMessage(It.Is<CreateOrderResponse>(r => !r.IsSuccess && r.Dto == null),
                _optionsKafka.Value!.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                @event.CorrelationId, null),
                Times.Once);
        }
    }
}
