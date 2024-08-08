using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.HtpClient.Interfaces;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.OrderService.Handlers;
using Test.Backend.Services.OrderService.Interfaces;
using Test.Backend.Services.OrderService.Mapper;
using Refit;

namespace Test.Backend.OrderService.XUnitTests.Handlers
{
    public class GetOrdersHandlerTests
    {
        private readonly Mock<IUserHttpClient> _userHttpClientMock;
        private readonly Mock<IProductHttpClient> _productHttpClientMock;
        private readonly Mock<IAddressHttpClient> _addressHttpClientMock;
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<ILogger<GetOrdersStartedHandler>> _loggerMock;
        private readonly IMapper _mapper;
        private readonly KafkaOptions _kafkaOptions;
        private readonly GetOrdersStartedHandler _handler;

        public GetOrdersHandlerTests()
        {
            // Arrange mocks
            _userHttpClientMock = new Mock<IUserHttpClient>();
            _productHttpClientMock = new Mock<IProductHttpClient>();
            _addressHttpClientMock = new Mock<IAddressHttpClient>();
            _msgBusMock = new Mock<IEventBusService>();
            _orderServiceMock = new Mock<IOrderService>();
            _loggerMock = new Mock<ILogger<GetOrdersStartedHandler>>();

            // Configure AutoMapper
            var config = new MapperConfiguration(cfg => cfg.AddProfile<Mapping>());
            _mapper = config.CreateMapper();

            _kafkaOptions = new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            };

            _handler = new GetOrdersStartedHandler(
                _msgBusMock.Object,
                _orderServiceMock.Object,
                _mapper,
                _userHttpClientMock.Object,
                _productHttpClientMock.Object,
                _addressHttpClientMock.Object,
                Options.Create(_kafkaOptions),
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task HandleAsync_Successful()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var addressId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var orders = new List<Order>
            {
                new Order
                {
                    Id = orderId,
                    UserId = userId,
                    DeliveryAddressId = addressId,
                    OrderProducts = new List<OrderProduct>
                    {
                        new OrderProduct { ProductId = productId }
                    }
                }
            };

            var userDto = new UserDto { Id = userId };
            var addressDto = new AddressDto { Id = addressId };
            var productDto = new ProductDto { Id = productId };

            var eventMessage = new GetOrdersStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                Activity = new { }
            };

            var expectedResponse = new GetOrdersResponse
            {
                IsSuccess = true,
                Dto = new List<OrderDto>
                {
                    new OrderDto
                    {
                        Id = orderId,
                        User = userDto,
                        Address = addressDto,
                        Products = new List<ProductDto> { productDto }
                    }
                }
            };

            _orderServiceMock.Setup(s => s.GetAsync()).ReturnsAsync(orders);

            _userHttpClientMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<UserDto>(new(), new UserDto { Id = userId }, new(), null));

            _addressHttpClientMock.Setup(x => x.GetAddressByIdAsync(addressId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<AddressDto>(new(), new AddressDto { Id = addressId }, new(), null));

            _productHttpClientMock.Setup(x => x.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<ProductDto>(new(), new ProductDto { Id = productId }, new(), null));

            // Act
            await _handler.HandleAsync(eventMessage);

            // Assert
            _msgBusMock.Verify(x => x.SendMessage(It.Is<GetOrdersResponse>(response =>
                response.IsSuccess == expectedResponse.IsSuccess &&
                response.Dto.Count == expectedResponse.Dto.Count &&
                response.Dto.First().Id == expectedResponse.Dto.First().Id), 
                _kafkaOptions.Producers!.ConsumerTopic!, 
                It.IsAny<CancellationToken>(), 
                eventMessage.CorrelationId, 
                null), 
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var eventMessage = new GetOrdersStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                Activity = new { }
            };

            _orderServiceMock.Setup(s => s.GetAsync()).ReturnsAsync(new List<Order>());

            // Act
            await _handler.HandleAsync(eventMessage);

            // Assert
            _msgBusMock.Verify(x => x.SendMessage(It.Is<GetOrdersResponse>(response =>
                !response.IsSuccess &&
                response.Dto == null), 
                _kafkaOptions.Producers!.ConsumerTopic!, 
                It.IsAny<CancellationToken>(), 
                eventMessage.CorrelationId,
                null), 
                Times.Once);
        }
    }
}
