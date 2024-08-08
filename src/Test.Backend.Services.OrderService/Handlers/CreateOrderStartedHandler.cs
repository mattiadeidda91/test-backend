using AutoMapper;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.HtpClient.Interfaces;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.OrderService.Interfaces;

namespace Test.Backend.Services.OrderService.Handlers
{
    public class CreateOrderStartedHandler : IEventHandler<CreateOrderStartedEvent>
    {
        private readonly IUserHttpClient userHttpClient;
        private readonly IProductHttpClient productHttpClient;
        private readonly IAddressHttpClient addressHttpClient;
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;
        private readonly ILogger<CreateOrderStartedHandler> logger;

        public CreateOrderStartedHandler(IEventBusService msgBus, IOrderService orderService,
            IUserHttpClient userHttpClient, IProductHttpClient productHttpClient, IAddressHttpClient addressHttpClient,
            IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<CreateOrderStartedHandler> logger)
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

        public async Task HandleAsync(CreateOrderStartedEvent @event)
        {
            logger.LogInformation($"Handling CreateOrderStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

            CreateOrderResponse response = new()
            {
                IsSuccess = false,
                Dto = null
            };

            var order = mapper.Map<Order>(@event.Activity);
            var alreadyExists = false;

            if (order != null)
            {
                if (order.Id != Guid.Empty)
                {
                    var orderDB = await orderService.GetByIdAsync(order.Id);
                    if (orderDB != null)
                    {
                        alreadyExists = true;
                        await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
                    }
                }

                if (!alreadyExists)
                {
                    var orderCanCreate = await CheckExistingEntities(order.UserId, order.DeliveryAddressId, @event.Activity?.ProductIds?.ToList() ?? new List<Guid>());

                    if (orderCanCreate)
                    {
                        await orderService.SaveAsync(order);

                        response.IsSuccess = true;
                        response.Dto = mapper.Map<OrderDto>(order);
                    }
                }
            }

            await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
        }

        private async Task<bool> CheckExistingEntities(Guid userId, Guid addressId, List<Guid> productsId)
        {
            var userDB = await userHttpClient.GetUserByIdAsync(userId);

            if(userDB.Content == null) return false;

            var addressDb = await addressHttpClient.GetAddressByIdAsync(addressId);

            if (addressDb.Content == null) return false;

            if(!productsId.Any()) return false;

            foreach (var productId in productsId)
            {
                var productDb = await productHttpClient.GetProductByIdAsync(productId);

                if(productDb.Content == null) return false;
            }

            return true;
        }
    }
}
