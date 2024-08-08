using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Refit;
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
using Test.Backend.Abstractions.Models.Dto.Order.Request;

namespace Test.Backend.OrderService.XUnitTests.Handlers
{
    public class GetOrderHandlerTests
    {
        private readonly Mock<IUserHttpClient> _userHttpClientMock;
        private readonly Mock<IProductHttpClient> _productHttpClientMock;
        private readonly Mock<IAddressHttpClient> _addressHttpClientMock;
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<ILogger<GetOrderStartedHandler>> _loggerMock;
        private readonly IMapper _mapper;
        private readonly KafkaOptions _kafkaOptions;
        private readonly GetOrderStartedHandler _handler;

        public GetOrderHandlerTests()
        {
            // Arrange mocks
            _userHttpClientMock = new Mock<IUserHttpClient>();
            _productHttpClientMock = new Mock<IProductHttpClient>();
            _addressHttpClientMock = new Mock<IAddressHttpClient>();
            _msgBusMock = new Mock<IEventBusService>();
            _orderServiceMock = new Mock<IOrderService>();
            _loggerMock = new Mock<ILogger<GetOrderStartedHandler>>();

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

            _handler = new GetOrderStartedHandler(
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

            var order = new Order
            {
                Id = orderId,
                UserId = userId,
                DeliveryAddressId = addressId,
                OrderProducts = new List<OrderProduct>
                {
                    new OrderProduct { ProductId = productId }
                }
            };

            var userDto = new UserDto { Id = userId };
            var addressDto = new AddressDto { Id = addressId };
            var productDto = new ProductDto { Id = productId };

            var eventMessage = new GetOrderStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                Activity = new OrderRequest(){ Id = orderId }
            };

            var expectedResponse = new GetOrderResponse
            {
                IsSuccess = true,
                Dto = new OrderDto
                {
                    Id = orderId,
                    User = userDto,
                    Address = addressDto,
                    Products = new List<ProductDto> { productDto }
                }
            };

            _orderServiceMock.Setup(s => s.GetByIdAsync(orderId)).ReturnsAsync(order);

            _userHttpClientMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<UserDto>(new(), new UserDto { Id = userId }, new(), null));

            _addressHttpClientMock.Setup(x => x.GetAddressByIdAsync(addressId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<AddressDto>(new(), new AddressDto { Id = addressId }, new(), null));

            _productHttpClientMock.Setup(x => x.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<ProductDto>(new(), new ProductDto { Id = productId }, new(), null));

            // Act
            await _handler.HandleAsync(eventMessage);

            // Assert
            _msgBusMock.Verify(x => x.SendMessage(It.Is<GetOrderResponse>(response =>
                response.IsSuccess == expectedResponse.IsSuccess &&
                response.Dto.Id == expectedResponse.Dto.Id &&
                response.Dto.User.Id == expectedResponse.Dto.User.Id &&
                response.Dto.Address.Id == expectedResponse.Dto.Address.Id &&
                response.Dto.Products.Count == expectedResponse.Dto.Products.Count &&
                response.Dto.Products.First().Id == expectedResponse.Dto.Products.First().Id),
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
            var orderId = Guid.NewGuid();
            var eventMessage = new GetOrderStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                Activity = new OrderRequest { Id = orderId }
            };

            _orderServiceMock.Setup(s => s.GetByIdAsync(orderId)).ReturnsAsync((Order)null);

            // Act
            await _handler.HandleAsync(eventMessage);

            // Assert
            _msgBusMock.Verify(x => x.SendMessage(It.Is<GetOrderResponse>(response =>
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
