using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Refit;
using System.Net;
using System.Reflection;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.HtpClient.Interfaces;
using Test.Backend.Services.OrderService.Controllers.v1;
using Test.Backend.Services.OrderService.Interfaces;
using Test.Backend.Services.OrderService.Mapper;

namespace Test.Backend.OrderService.XUnitTests.Controller
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IUserHttpClient> _userHttpClientMock;
        private readonly Mock<IProductHttpClient> _productHttpClientMock;
        private readonly Mock<IAddressHttpClient> _addressHttpClientMock;
        private readonly Mock<ILogger<OrderController>> _loggerMock;
        private readonly IMapper _mapper;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            // Arrange mocks
            _orderServiceMock = new Mock<IOrderService>();
            _userHttpClientMock = new Mock<IUserHttpClient>();
            _productHttpClientMock = new Mock<IProductHttpClient>();
            _addressHttpClientMock = new Mock<IAddressHttpClient>();
            _loggerMock = new Mock<ILogger<OrderController>>();

            // Configure AutoMapper
            var config = new MapperConfiguration(cfg => cfg.AddProfile<Mapping>());
            _mapper = config.CreateMapper();

            _controller = new OrderController(
                _orderServiceMock.Object,
                _userHttpClientMock.Object,
                _productHttpClientMock.Object,
                _addressHttpClientMock.Object,
                _loggerMock.Object,
                _mapper
            );
        }

        [Fact]
        public async Task GetOrdersEntities_Single_Successful()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var addressId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var userDto = new UserDto()
            {
                Id = userId,
            };

            var addressDto = new AddressDto()
            {
                Id = addressId,
            };

            var productDto = new ProductDto()
            {
                Id = productId,
            };

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

            var expectedOrderDto = new OrderDto
            {
                Id = orderId,
                User = new UserDto { Id = userId },
                Address = new AddressDto { Id = addressId },
                Products = new List<ProductDto> { new ProductDto { Id = productId } }
            };

            // Mock HTTP client responses
            _userHttpClientMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<UserDto>(new(), userDto, new(), null));

            _addressHttpClientMock.Setup(x => x.GetAddressByIdAsync(addressId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<AddressDto>(new(), addressDto, new(), null));

            _productHttpClientMock.Setup(x => x.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<ProductDto>(new(), productDto, new(), null));

            var methodInfo = typeof(OrderController).GetMethod("GetOrdersEntities", 
                BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(Order), typeof(CancellationToken) }, null);

            if (methodInfo == null)
            {
                throw new InvalidOperationException("Method GetOrdersEntities not found.");
            }

            // Act
            var result = await (Task<OrderDto>)methodInfo.Invoke(_controller, new object[] { order, CancellationToken.None });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOrderDto.Id, result.Id);
            Assert.Equal(expectedOrderDto.User.Id, result.User.Id);
            Assert.Equal(expectedOrderDto.Address.Id, result.Address.Id);
            Assert.Single(result.Products);
            Assert.Equal(expectedOrderDto.Products.First().Id, result.Products.First().Id);
        }

        [Fact]
        public async Task GetOrdersEntities_Multiple_Successful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var addressId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var userDto = new UserDto()
            {
                Id = userId,
            };

            var addressDto = new AddressDto()
            {
                Id = addressId,
            };

            var productDto = new ProductDto()
            {
                Id = productId,
            };

            var orders = new List<Order>
            {
                new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    DeliveryAddressId = addressId,
                    OrderProducts = new List<OrderProduct>
                    {
                        new OrderProduct { ProductId = productId }
                    }
                }
            };

            var expectedOrderDtos = orders.Select(order => new OrderDto
            {
                Id = order.Id,
                User = new UserDto { Id = userId },
                Address = new AddressDto { Id = addressId },
                Products = new List<ProductDto> { new ProductDto { Id = productId } }
            }).ToList();

            // Mock HTTP client responses
            _userHttpClientMock
                .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ApiResponse<UserDto>(new(), userDto, new(), null));

            _addressHttpClientMock
                .Setup(x => x.GetAddressByIdAsync(addressId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<AddressDto>(new(), addressDto, new(), null));

            _productHttpClientMock
                .Setup(x => x.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<ProductDto>(new(), productDto, new(), null));

            var methodInfo = typeof(OrderController).GetMethod("GetOrdersEntities", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IEnumerable<Order>), typeof(CancellationToken) }, null);
            if (methodInfo == null)
            {
                throw new InvalidOperationException("Method GetOrdersEntities not found.");
            }

            // Act
            var result = await (Task<List<OrderDto>>)methodInfo.Invoke(_controller, new object[] { orders, CancellationToken.None });

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var resultDto = result.First();
            var expectedDto = expectedOrderDtos.First();
            Assert.Equal(expectedDto.Id, resultDto.Id);
            Assert.Equal(expectedDto.User.Id, resultDto.User.Id);
            Assert.Equal(expectedDto.Address.Id, resultDto.Address.Id);
            Assert.Single(resultDto.Products);
            Assert.Equal(expectedDto.Products.First().Id, resultDto.Products.First().Id);
        }

        [Fact]
        public async Task GetOrders_Successful()
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

            var orderDto = new OrderDto
            {
                Id = orderId,
                User = new UserDto { Id = userId },
                Address = new AddressDto { Id = addressId },
                Products = new List<ProductDto> { new ProductDto { Id = productId } }
            };

            _orderServiceMock.Setup(s => s.GetAsync()).ReturnsAsync(new List<Order> { order });

            _userHttpClientMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<UserDto>(new(), new UserDto { Id = userId }, new(), null));

            _addressHttpClientMock.Setup(x => x.GetAddressByIdAsync(addressId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<AddressDto>(new(), new AddressDto { Id = addressId }, new(), null));

            _productHttpClientMock.Setup(x => x.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<ProductDto>(new(), new ProductDto { Id = productId }, new(), null));

            // Act
            var result = await _controller.GetOrders(CancellationToken.None) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var resultValue = result.Value as List<OrderDto>;
            Assert.NotNull(resultValue);
            Assert.Single(resultValue);
            var resultOrderDto = resultValue.First();
            Assert.Equal(orderDto.Id, resultOrderDto.Id);
            Assert.Equal(orderDto.User.Id, resultOrderDto.User.Id);
            Assert.Equal(orderDto.Address.Id, resultOrderDto.Address.Id);
            Assert.Single(resultOrderDto.Products);
            Assert.Equal(orderDto.Products.First().Id, resultOrderDto.Products.First().Id);
        }

        [Fact]
        public async Task GetOrders_Fails()
        {
            // Arrange
            var orders = new List<Order>();

            _orderServiceMock.Setup(s => s.GetAsync()).ReturnsAsync(orders);

            // Act
            var result = await _controller.GetOrders(CancellationToken.None) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
            Assert.Empty(result.Value as IEnumerable<OrderDto>);
        }

        [Fact]
        public async Task GetOrderById_Successful()
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

            var orderDto = new OrderDto
            {
                Id = orderId,
                User = new UserDto { Id = userId },
                Address = new AddressDto { Id = addressId },
                Products = new List<ProductDto> { new ProductDto { Id = productId } }
            };

            _orderServiceMock.Setup(s => s.GetByIdAsync(orderId)).ReturnsAsync(order);

            _userHttpClientMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<UserDto>(new(), new UserDto { Id = userId }, new(), null));

            _addressHttpClientMock.Setup(x => x.GetAddressByIdAsync(addressId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<AddressDto>(new(), new AddressDto { Id = addressId }, new(), null));

            _productHttpClientMock.Setup(x => x.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<ProductDto>(new(), new ProductDto { Id = productId }, new(), null));

            // Act
            var result = await _controller.GetOrderById(orderId, CancellationToken.None) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var resultValue = result.Value as OrderDto;
            Assert.NotNull(resultValue);
            Assert.Equal(orderDto.Id, resultValue.Id);
            Assert.Equal(orderDto.User.Id, resultValue.User.Id);
            Assert.Equal(orderDto.Address.Id, resultValue.Address.Id);
            Assert.Single(resultValue.Products);
            Assert.Equal(orderDto.Products.First().Id, resultValue.Products.First().Id);
        }

        [Fact]
        public async Task GetOrderById_Fails()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _orderServiceMock.Setup(s => s.GetByIdAsync(orderId)).ReturnsAsync((Order)null);

            // Act
            var result = await _controller.GetOrderById(orderId, CancellationToken.None) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
            Assert.Equal("Order not found.", result.Value);
        }
    }
}
