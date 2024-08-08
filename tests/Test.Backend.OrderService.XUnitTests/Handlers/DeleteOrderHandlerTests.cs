using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Order.Request;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.OrderService.Handlers;
using Test.Backend.Services.OrderService.Interfaces;

namespace Test.Backend.OrderService.XUnitTests.Handlers
{
    public class DeleteOrderHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<DeleteOrderStartedHandler>> _loggerMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly DeleteOrderStartedHandler _handler;

        public DeleteOrderHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _orderServiceMock = new Mock<IOrderService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<DeleteOrderStartedHandler>>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _kafkaOptionsMock.Setup(o => o.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });
            _handler = new DeleteOrderStartedHandler(
                _msgBusMock.Object,
                _orderServiceMock.Object,
                _mapperMock.Object,
                _kafkaOptionsMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task HandleAsync_Successful()
        {
            // Arrange
            var deleteOrderEvent = new DeleteOrderStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new OrderRequest
                {
                    Id = Guid.NewGuid()
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var order = new Order
            {
                Id = deleteOrderEvent.Activity.Id,
            };

            var orderDto = new OrderDto
            {
                Id = order.Id,
            };

            _orderServiceMock.Setup(s => s.GetByIdAsync(deleteOrderEvent.Activity.Id)).ReturnsAsync(order);
            _orderServiceMock.Setup(s => s.DeleteAsync(deleteOrderEvent.Activity.Id)).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<OrderDto>(order)).Returns(orderDto);

            // Act
            await _handler.HandleAsync(deleteOrderEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<DeleteOrderResponse>(r => r.IsSuccess && r.Dto == orderDto),
                _kafkaOptionsMock.Object.Value.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                deleteOrderEvent.CorrelationId,
                null
            ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails_Null()
        {
            // Arrange
            var deleteOrderEvent = new DeleteOrderStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new OrderRequest
                {
                    Id = Guid.NewGuid()
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            _orderServiceMock.Setup(s => s.GetByIdAsync(deleteOrderEvent.Activity.Id)).ReturnsAsync((Order)null);

            // Act
            await _handler.HandleAsync(deleteOrderEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<DeleteOrderResponse>(r => !r.IsSuccess && r.Dto == null),
                _kafkaOptionsMock.Object.Value.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                deleteOrderEvent.CorrelationId,
                null
            ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var deleteOrderEvent = new DeleteOrderStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new OrderRequest
                {
                    Id = Guid.NewGuid()
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var order = new Order
            {
                Id = deleteOrderEvent.Activity.Id,
                // other properties
            };

            _orderServiceMock.Setup(s => s.GetByIdAsync(deleteOrderEvent.Activity.Id)).ReturnsAsync(order);
            _orderServiceMock.Setup(s => s.DeleteAsync(deleteOrderEvent.Activity.Id)).ReturnsAsync(false);

            // Act
            await _handler.HandleAsync(deleteOrderEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<DeleteOrderResponse>(r => !r.IsSuccess && r.Dto == null),
                _kafkaOptionsMock.Object.Value.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                deleteOrderEvent.CorrelationId,
                null
            ), Times.Once);
        }
    }
}
