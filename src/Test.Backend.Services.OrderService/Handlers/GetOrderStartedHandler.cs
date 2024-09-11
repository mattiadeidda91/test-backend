using AutoMapper;
using Microsoft.Extensions.Options;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.User.Response;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Events.User;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.OrderService.Interfaces;
using Test.Backend.Abstractions.Models.Events.Order;
using System.Text.Json;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.OrderService.Utils;
using Test.Backend.HtpClient.Interfaces;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Abstractions.Costants;

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.

namespace Test.Backend.Services.OrderService.Handlers
{
    public class GetOrderStartedHandler : IEventHandler<GetOrderStartedEvent>
    {
        private readonly IUserHttpClient userHttpClient;
        private readonly IProductHttpClient productHttpClient;
        private readonly IAddressHttpClient addressHttpClient;
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;
        private readonly ILogger<GetOrderStartedHandler> logger;

        public GetOrderStartedHandler(IEventBusService msgBus, IOrderService orderService, IMapper mapper,
             IUserHttpClient userHttpClient, IProductHttpClient productHttpClient, IAddressHttpClient addressHttpClient,
            IOptions<KafkaOptions> kafkaOptions, ILogger<GetOrderStartedHandler> logger)
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

        public async Task HandleAsync(GetOrderStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<GetOrderResponse, OrderDto>(
                async () =>
                {
                    logger.LogInformation($"Handling GetOrderStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                    GetOrderResponse response = new()
                    {
                        IsSuccess = false,
                        Dto = null
                    };

                    var order = await orderService.GetByIdAsync(@event.Activity!.Id);

                    if (order != null)
                    {
                        var orderDto = await GetOrdersEntities(order, new CancellationToken());

                        response.IsSuccess = true;
                        response.Dto = orderDto;
                    }
                    else
                    {
                        response.ReturnCode = 404;
                        response.Messsage = string.Format(ResponseMessages.GetByIdNotFound, "Order", @event.Activity!.Id);
                    }

                    await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);

                    return response;
                },
                msgBus,
                kafkaOptions.Producers!.ConsumerTopic!,
                @event.CorrelationId!,
                logger);
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
    }
}

#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
