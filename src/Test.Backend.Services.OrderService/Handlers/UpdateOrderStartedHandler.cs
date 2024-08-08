using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.HtpClient.Interfaces;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.OrderService.Interfaces;

namespace Test.Backend.Services.OrderService.Handlers
{
    public class UpdateOrderStartedHandler : IEventHandler<UpdateOrderStartedEvent>
    {
        private readonly IUserHttpClient userHttpClient;
        private readonly IProductHttpClient productHttpClient;
        private readonly IAddressHttpClient addressHttpClient;
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IOrderService orderService;
        private readonly IOrderProductService orderProductService;
        private readonly IMapper mapper;
        private readonly ILogger<UpdateOrderStartedHandler> logger;

        public UpdateOrderStartedHandler(IEventBusService msgBus, IOrderService orderService, IOrderProductService orderProductService,
            IUserHttpClient userHttpClient, IProductHttpClient productHttpClient, IAddressHttpClient addressHttpClient,
            IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<UpdateOrderStartedHandler> logger)
        {
            this.productHttpClient = productHttpClient;
            this.addressHttpClient = addressHttpClient;
            this.userHttpClient = userHttpClient;
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.orderService = orderService;
            this.orderProductService = orderProductService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(UpdateOrderStartedEvent @event)
        {
            logger.LogInformation($"Handling UpdateOrderStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

            UpdateOrderResponse response = new()
            {
                IsSuccess = false,
                Dto = null
            };

            var orderDb = await orderService.GetByIdAsync(@event.Activity!.Id);

            if (orderDb != null)
            {
                foreach(var orderProduct in orderDb.OrderProducts.ToList())
                {
                    _ = await orderProductService.DeleteAsync(orderProduct);
                }

                var order = mapper.Map(@event.Activity, orderDb);

                if (order != null)
                {
                    var orderCanCreate = await CheckExistingEntities(order.UserId, order.DeliveryAddressId, @event.Activity?.ProductIds?.ToList() ?? new List<Guid>());

                    if (orderCanCreate)
                    {
                        await orderService.UpdateAsync(order);

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

            if (userDB.Content == null) return false;

            var addressDb = await addressHttpClient.GetAddressByIdAsync(addressId);

            if (addressDb.Content == null) return false;

            if (!productsId.Any()) return false;

            foreach (var productId in productsId)
            {
                var productDb = await productHttpClient.GetProductByIdAsync(productId);

                if (productDb.Content == null) return false;
            }

            return true;
        }
    }
}
