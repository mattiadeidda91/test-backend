using AutoMapper;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Request;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.OrderService.Mapper;

namespace Test.Backend.OrderService.XUnitTests.Mapper
{
    public class MappingTests
    {
        private readonly IMapper _mapper;

        public MappingTests()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Mapping>();
            });

            _mapper = configuration.CreateMapper();
        }

        [Fact]
        public void Map_Order_To_OrderRequest()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                DeliveryAddressId = Guid.NewGuid(),
                OrderDate = DateTime.UtcNow,
                OrderProducts = new List<OrderProduct>
            {
                new OrderProduct { ProductId = Guid.NewGuid(), OrderId =  Guid.NewGuid() },
                new OrderProduct { ProductId = Guid.NewGuid(), OrderId =  Guid.NewGuid() }
            }
            };

            // Act
            var orderRequest = _mapper.Map<OrderRequest>(order);

            // Assert
            Assert.NotNull(orderRequest);
            Assert.Equal(order.UserId, orderRequest.UserId);
            Assert.Equal(order.DeliveryAddressId, orderRequest.DeliveryAddressId);
            Assert.Equal(order.OrderDate, orderRequest.OrderDate);
            Assert.Null(orderRequest.ProductIds);
        }

        [Fact]
        public void Map_OrderRequest_To_Order()
        {
            // Arrange
            var orderRequest = new OrderRequest
            {
                UserId = Guid.NewGuid(),
                DeliveryAddressId = Guid.NewGuid(),
                ProductIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
                OrderDate = DateTime.UtcNow
            };

            // Act
            var order = _mapper.Map<Order>(orderRequest);

            // Assert
            Assert.NotNull(order);
            Assert.Equal(orderRequest.UserId, order.UserId);
            Assert.Equal(orderRequest.DeliveryAddressId, order.DeliveryAddressId);
            Assert.Equal(orderRequest.OrderDate, order.OrderDate);
            Assert.NotNull(order.OrderProducts);
            Assert.Equal(orderRequest.ProductIds.Count(), order.OrderProducts.Count);
        }

        [Fact]
        public void Map_OrderBaseDto_To_Order()
        {
            // Arrange
            var orderBaseDto = new OrderBaseDto
            {
                Id = Guid.NewGuid(),
                OrderDate = DateTime.UtcNow
            };

            // Act
            var order = _mapper.Map<Order>(orderBaseDto);

            // Assert
            Assert.NotNull(order);
            Assert.Equal(orderBaseDto.OrderDate, order.OrderDate);
        }

        [Fact]
        public void Map_Order_To_OrderBaseDto()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                DeliveryAddressId= Guid.NewGuid(),
                UserId= Guid.NewGuid(), 
                OrderDate = DateTime.UtcNow,
            };

            // Act
            var orderBaseDto = _mapper.Map<OrderBaseDto>(order);

            // Assert
            Assert.NotNull(orderBaseDto);
            Assert.Equal(order.OrderDate, orderBaseDto.OrderDate);
        }

        [Fact]
        public void Map_Order_To_OrderDto()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                DeliveryAddressId = Guid.NewGuid(),
                OrderDate = DateTime.UtcNow,
                OrderProducts = new List<OrderProduct>
                {
                    new OrderProduct { OrderId = Guid.NewGuid(), Order = new(){ Id = Guid.NewGuid() } },
                    new OrderProduct {OrderId = Guid.NewGuid(), Order = new() { Id = Guid.NewGuid() }}
                }
            };

            // Act
            var orderDto = _mapper.Map<OrderDto>(order);

            // Assert
            Assert.NotNull(orderDto);
            Assert.Equal(order.OrderDate, orderDto.OrderDate);
            Assert.Null(orderDto.Products);
        }

        [Fact]
        public void Map_Order_To_OrderWithoutAddressDto()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                OrderProducts = new List<OrderProduct>
                {
                    new OrderProduct {OrderId = Guid.NewGuid(), Order = new() { Id = Guid.NewGuid() }}
                }
            };

            // Act
            var orderWithoutAddressDto = _mapper.Map<OrderWithoutAddressDto>(order);

            // Assert
            Assert.NotNull(orderWithoutAddressDto);
            Assert.Null(orderWithoutAddressDto.Products);
        }

        [Fact]
        public void Map_Order_To_OrderWithoutUserDto()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                DeliveryAddressId = Guid.NewGuid(),
                OrderProducts = new List<OrderProduct>
                {
                    new OrderProduct {OrderId = Guid.NewGuid(), Order = new() { Id = Guid.NewGuid() }}
                }
            };

            // Act
            var orderWithoutUserDto = _mapper.Map<OrderWithoutUserDto>(order);

            // Assert
            Assert.NotNull(orderWithoutUserDto);
            Assert.Null(orderWithoutUserDto.Products);
        }

        [Fact]
        public void Map_Order_To_OrderWithoutProductDto()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                DeliveryAddressId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var orderWithoutProductDto = _mapper.Map<OrderWithoutProductDto>(order);

            // Assert
            Assert.NotNull(orderWithoutProductDto);
        }
    }
}
