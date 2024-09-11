using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.OrderService.Interfaces;

namespace Test.Backend.Services.OrderService.Handlers
{
    public class DeleteOrderStartedHandler : IEventHandler<DeleteOrderStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;
        private readonly ILogger<DeleteOrderStartedHandler> logger;

        public DeleteOrderStartedHandler(IEventBusService msgBus, IOrderService orderService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<DeleteOrderStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.orderService = orderService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(DeleteOrderStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<DeleteOrderResponse, OrderDto>(
                async () =>
                {
                    logger.LogInformation($"Handling DeleteOrderStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                    DeleteOrderResponse response = new()
                    {
                        IsSuccess = false,
                        Dto = null
                    };

                    var orderDb = await orderService.GetByIdAsync(@event.Activity!.Id);

                    if (orderDb != null)
                    {
                        var isDeleted = await orderService.DeleteAsync(@event.Activity!.Id);

                        if (isDeleted)
                        {
                            response.IsSuccess = true;
                            response.Dto = mapper.Map<OrderDto>(orderDb);
                        }
                        else
                        {
                            response.ReturnCode = 500;
                            response.Messsage = string.Format(ResponseMessages.GenericError, "Order", "deleted");
                        }
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
    }
}
