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
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.User;

namespace Test.Backend.OrderService.XUnitTests.Handlers
{
    public  class UpdateOrderHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IOrderProductService> _orderProductServiceMock;
        private readonly Mock<IOrderEntitesService> _orderEntitesServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<UpdateOrderStartedHandler>> _loggerMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly UpdateOrderStartedHandler _handler;

        public UpdateOrderHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _orderServiceMock = new Mock<IOrderService>();
            _orderProductServiceMock = new Mock<IOrderProductService>();
            _orderEntitesServiceMock = new Mock<IOrderEntitesService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<UpdateOrderStartedHandler>>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _kafkaOptionsMock.Setup(o => o.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });
            _handler = new UpdateOrderStartedHandler(
                _msgBusMock.Object,
                _orderServiceMock.Object,
                _orderProductServiceMock.Object,
                _orderEntitesServiceMock.Object,
                _mapperMock.Object,
                _kafkaOptionsMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task HandleAsync_Successful()
        {
            // Arrange
            var updateOrderEvent = new UpdateOrderStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new OrderRequest
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    DeliveryAddressId = Guid.NewGuid(),
                    ProductIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var orderDb = new Order
            {
                Id = updateOrderEvent.Activity.Id,
                OrderProducts = new List<OrderProduct>
                {
                    new OrderProduct { OrderId = Guid.NewGuid(), ProductId = Guid.NewGuid() }
                }
            };

            var updatedOrder = new Order
            {
                Id = updateOrderEvent.Activity.Id,
                UserId = updateOrderEvent.Activity.UserId,
                DeliveryAddressId = updateOrderEvent.Activity.DeliveryAddressId
            };

            var orderDto = new OrderDto
            {
                Id = updatedOrder.Id,
                User = new UserDto { Id = updatedOrder.UserId, FirstName = "Test User" },
                Address = new AddressDto { Id = updatedOrder.DeliveryAddressId, Street = "Test Street" },
                Products = new List<ProductDto>
                {
                    new ProductDto { Id = Guid.NewGuid(), Name = "Product 1" },
                    new ProductDto { Id = Guid.NewGuid(), Name = "Product 2" }
                }
            };

            _orderServiceMock.Setup(s => s.GetByIdAsync(updateOrderEvent.Activity.Id)).ReturnsAsync(orderDb);
            _orderProductServiceMock.Setup(s => s.DeleteAsync(It.IsAny<OrderProduct>())).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map(It.IsAny<OrderRequest>(), It.IsAny<Order>())).Returns(updatedOrder);
            _mapperMock.Setup(m => m.Map<OrderDto>(updatedOrder)).Returns(orderDto);
            _orderEntitesServiceMock.Setup(s => s.CheckAndGetExistingEntities(
                updatedOrder.UserId,
                updatedOrder.DeliveryAddressId,
                updateOrderEvent.Activity.ProductIds.ToList()
            )).ReturnsAsync((true, new UserDto(), new AddressDto(), orderDto.Products.ToList()));
            
            _orderServiceMock.Setup(s => s.UpdateAsync(updatedOrder)).Returns(Task.CompletedTask);

            // Act
            await _handler.HandleAsync(updateOrderEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<UpdateOrderResponse>(r => r.IsSuccess && r.Dto == orderDto),
                _kafkaOptionsMock.Object.Value.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                updateOrderEvent.CorrelationId,
                null
            ), Times.Once);

            _orderProductServiceMock.Verify(m => m.DeleteAsync(It.IsAny<OrderProduct>()), Times.Exactly(orderDb.OrderProducts.Count));
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var updateOrderEvent = new UpdateOrderStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new OrderRequest
                {
                    Id = Guid.NewGuid()
                },
                CorrelationId = Guid.NewGuid().ToString()
            };

            _orderServiceMock.Setup(s => s.GetByIdAsync(updateOrderEvent.Activity.Id)).ReturnsAsync((Order)null);

            // Act
            await _handler.HandleAsync(updateOrderEvent);

            // Assert
            _msgBusMock.Verify(m => m.SendMessage(
                It.Is<UpdateOrderResponse>(r => !r.IsSuccess && r.Dto == null),
                _kafkaOptionsMock.Object.Value!.Producers!.ConsumerTopic,
                It.IsAny<CancellationToken>(),
                updateOrderEvent.CorrelationId,
                null
            ), Times.Once);

            _orderProductServiceMock.Verify(m => m.DeleteAsync(It.IsAny<OrderProduct>()), Times.Never);
        }

    }
}
