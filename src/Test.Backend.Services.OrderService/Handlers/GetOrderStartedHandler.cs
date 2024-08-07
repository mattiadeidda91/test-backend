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

namespace Test.Backend.Services.OrderService.Handlers
{
    public class GetOrderStartedHandler : IEventHandler<GetOrderStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;
        private readonly ILogger<GetOrderStartedHandler> logger;

        public GetOrderStartedHandler(IEventBusService msgBus, IOrderService orderService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<GetOrderStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.orderService = orderService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(GetOrderStartedEvent @event)
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
                response.IsSuccess = true;
                response.Dto = mapper.Map<OrderDto>(order);
            }

            //TODO: implement call to OrderService to retrieve others info

            await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
        }
    }

}
