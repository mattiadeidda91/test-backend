using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.OrderService.Interfaces;

namespace Test.Backend.Services.OrderService.Handlers
{
    public class GetOrdersStartedHandler : IEventHandler<GetOrdersStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;
        private readonly ILogger<GetOrdersStartedHandler> logger;

        public GetOrdersStartedHandler(IEventBusService msgBus, IOrderService orderService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<GetOrdersStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.orderService = orderService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(GetOrdersStartedEvent @event)
        {
            logger.LogInformation($"Handling GetOrdersStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

            GetOrdersResponse response = new()
            {
                IsSuccess = false,
                Dto = null
            };

            var users = await orderService.GetAsync();

            if (users.Any())
            {
                response.IsSuccess = true;
                response.Dto = mapper.Map<List<OrderDto>>(users);
            }

            //TODO: implement call to OrderService to retrieve others info

            await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
        }
    }
}
