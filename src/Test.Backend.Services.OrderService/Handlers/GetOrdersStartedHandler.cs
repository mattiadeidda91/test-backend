using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.Dependencies.Utils;
using Test.Backend.HtpClient.Interfaces;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.OrderService.Interfaces;
using Test.Backend.Services.OrderService.Utils;

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.

namespace Test.Backend.Services.OrderService.Handlers
{
    public class GetOrdersStartedHandler : IEventHandler<GetOrdersStartedEvent>
    {
        private readonly IUserHttpClient userHttpClient;
        private readonly IProductHttpClient productHttpClient;
        private readonly IAddressHttpClient addressHttpClient;
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;
        private readonly ILogger<GetOrdersStartedHandler> logger;

        public GetOrdersStartedHandler(IEventBusService msgBus, IOrderService orderService, IMapper mapper,
            IUserHttpClient userHttpClient, IProductHttpClient productHttpClient, IAddressHttpClient addressHttpClient,
            IOptions<KafkaOptions> kafkaOptions, ILogger<GetOrdersStartedHandler> logger)
        {
            this.productHttpClient = productHttpClient;
            this.addressHttpClient = addressHttpClient;
            this.userHttpClient = userHttpClient;
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.orderService = orderService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(GetOrdersStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<GetOrdersResponse, List<OrderDto>>(
                async () =>
                {
                    logger.LogInformation($"Handling GetOrdersStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                    GetOrdersResponse response = new()
                    {
                        IsSuccess = false,
                        Dto = null
                    };

                    var orders = await orderService.GetAsync();

                    if (orders.Any())
                    {
                        var ordersDto = await GetOrdersEntities(orders, new CancellationToken());

                        response.IsSuccess = true;
                        response.Dto = ordersDto;
                    }
                    else
                    {
                        response.ReturnCode = 404;
                        response.Message = string.Format(ResponseMessages.GetNotFound, "Orders");
                    }

                    await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);

                    return response;
                },
                msgBus,
                kafkaOptions.Producers!.ConsumerTopic!,
                @event.CorrelationId!,
                logger);
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