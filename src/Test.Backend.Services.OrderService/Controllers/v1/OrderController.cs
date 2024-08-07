using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.HtpClient.Interfaces;
using Test.Backend.Services.OrderService.Interfaces;
using Test.Backend.Services.OrderService.Utils;

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.

namespace Test.Backend.Services.OrderService.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IUserHttpClient userHttpClient;
        private readonly IProductHttpClient productHttpClient;
        private readonly IAddressHttpClient addressHttpClient;
        private readonly ILogger<OrderController> logger;
        private readonly IMapper mapper;
        private readonly IOrderService orderService;

        public OrderController(IOrderService orderService, IUserHttpClient userHttpClient, IProductHttpClient productHttpClient, IAddressHttpClient addressHttpClient,
            ILogger<OrderController> logger, IMapper mapper)
        {
            this.productHttpClient= productHttpClient;
            this.addressHttpClient = addressHttpClient;
            this.userHttpClient = userHttpClient;
            this.orderService = orderService;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
        {
            var orders = await orderService.GetAsync();

           var ordersDto = await GetOrdersEntities(orders, cancellationToken);

            return StatusCode(ordersDto.Any() ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, ordersDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById([Required] Guid id, CancellationToken cancellationToken)
        {
            var order = await orderService.GetByIdAsync(id);

            if (order == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Order not found.");
            }

            var orderDto = await GetOrdersEntities(order, cancellationToken);

            return StatusCode(StatusCodes.Status200OK, orderDto);
        }

        private async Task<OrderDto> GetOrdersEntities(Order order, CancellationToken cancellationToken)
        {
            var userId = order.UserId;
            var addressId = order.DeliveryAddressId;
            var productIds = order.OrderProducts.Select(op => op.ProductId).Distinct().ToList();

            var userDto = await UtilityClient.FetchEntityAsync(userId, id => userHttpClient.GetUserByIdAsync(id, cancellationToken), cancellationToken);
            var addressDto = await UtilityClient.FetchEntityAsync(addressId, id => addressHttpClient.GetAddressByIdAsync(id, cancellationToken), cancellationToken);
            var productsDto = await UtilityClient.FetchEntitiesAsync(productIds, id => productHttpClient.GetProductByIdAsync(id, cancellationToken), cancellationToken);

            var orderDto = mapper.Map<OrderDto>(order);

            if (userDto != null)
            {
                orderDto.User = userDto;
            }
            if (addressDto != null)
            {
                orderDto.Address = addressDto;
            }

            orderDto.Products = order.OrderProducts
                .Select(op => productsDto.TryGetValue(op.ProductId, out var productDto) ? productDto : null)
                .Where(dto => dto != null)
                .ToList();
            return orderDto;
        }

        private async Task<List<OrderDto>> GetOrdersEntities(IEnumerable<Order> orders, CancellationToken cancellationToken)
        {
            // Extract IDs for users, addresses, and products
            var userIds = orders.Select(order => order.UserId).Distinct().ToList();
            var addressIds = orders.Select(order => order.DeliveryAddressId).Distinct().ToList();
            var productIds = orders.SelectMany(order => order.OrderProducts.Select(op => op.ProductId)).Distinct().ToList();

            // Fetch DTOs
            var usersDto = await UtilityClient.FetchEntitiesAsync(userIds, id => userHttpClient.GetUserByIdAsync(id, cancellationToken), cancellationToken);
            var addressesDto = await UtilityClient.FetchEntitiesAsync(addressIds, id => addressHttpClient.GetAddressByIdAsync(id, cancellationToken), cancellationToken);
            var productsDto = await UtilityClient.FetchEntitiesAsync(productIds, id => productHttpClient.GetProductByIdAsync(id, cancellationToken), cancellationToken);

            // Map orders to DTOs and populate additional properties
            var ordersDto = orders.Select(order =>
            {
                var orderDto = mapper.Map<OrderDto>(order);

                //Populate User
                if (usersDto.TryGetValue(order.UserId, out var userDto))
                {
                    orderDto.User = userDto;
                }

                //Populate Address
                if (addressesDto.TryGetValue(order.DeliveryAddressId, out var addressDto))
                {
                    orderDto.Address = addressDto;
                }

                //Populate Products
                orderDto.Products = order.OrderProducts
                    .Select(op => productsDto.TryGetValue(op.ProductId, out var productDto) ? productDto : null)
                    .Where(dto => dto != null)
                    .ToList();

                return orderDto;
            }).ToList();
            return ordersDto;
        }
    }
}

#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
