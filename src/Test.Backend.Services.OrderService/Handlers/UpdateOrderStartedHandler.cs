using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.OrderService.Interfaces;

namespace Test.Backend.Services.OrderService.Handlers
{
    public class UpdateOrderStartedHandler : IEventHandler<UpdateOrderStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IOrderService orderService;
        private readonly IOrderProductService orderProductService;
        private readonly IMapper mapper;
        private readonly ILogger<UpdateOrderStartedHandler> logger;

        public UpdateOrderStartedHandler(IEventBusService msgBus, IOrderService orderService, IOrderProductService orderProductService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<UpdateOrderStartedHandler> logger)
        {
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
                    await orderService.UpdateAsync(order);

                    response.IsSuccess = true;
                    response.Dto = mapper.Map<OrderDto>(order);
                }
            }

            await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);

        }
    }
}
