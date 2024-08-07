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
    public class CreateOrderStartedHandler : IEventHandler<CreateOrderStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;
        private readonly ILogger<CreateOrderStartedHandler> logger;

        public CreateOrderStartedHandler(IEventBusService msgBus, IOrderService orderService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<CreateOrderStartedHandler> logger)
        {
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

            if (order != null)
            {
                await orderService.SaveAsync(order);

                response.IsSuccess = true;
                response.Dto = mapper.Map<OrderDto>(order);
            }

            await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
        }
    }
}
