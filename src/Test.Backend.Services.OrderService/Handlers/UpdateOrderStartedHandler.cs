using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.Dependencies.Utils;
using Test.Backend.HtpClient.Interfaces;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.OrderService.Interfaces;

namespace Test.Backend.Services.OrderService.Handlers
{
    public class UpdateOrderStartedHandler : IEventHandler<UpdateOrderStartedEvent>
    {
        private readonly IOrderEntitesService orderEntitesService;
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IOrderService orderService;
        private readonly IOrderProductService orderProductService;
        private readonly IMapper mapper;
        private readonly ILogger<UpdateOrderStartedHandler> logger;

        public UpdateOrderStartedHandler(IEventBusService msgBus, IOrderService orderService, IOrderProductService orderProductService,
            IOrderEntitesService orderEntitesService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<UpdateOrderStartedHandler> logger)
        {
            this.orderEntitesService = orderEntitesService;
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.orderService = orderService;
            this.orderProductService = orderProductService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(UpdateOrderStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<UpdateOrderResponse, OrderDto>(
                async () =>
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
                        foreach (var orderProduct in orderDb.OrderProducts.ToList())
                        {
                            _ = await orderProductService.DeleteAsync(orderProduct);
                        }

                        var order = mapper.Map(@event.Activity, orderDb);

                        if (order != null)
                        {
                            (var orderCanCreate, var userDto, var addressDto, var productsDto) = await orderEntitesService.CheckAndGetExistingEntities(order.UserId, order.DeliveryAddressId, @event.Activity?.ProductIds?.ToList() ?? new List<Guid>());

                            if (orderCanCreate)
                            {
                                await orderService.UpdateAsync(order);

                                var orderDto = mapper.Map<OrderDto>(order);
                                orderDto.User = userDto;
                                orderDto.Address = addressDto;
                                orderDto.Products = productsDto;

                                response.IsSuccess = true;
                                response.Dto = orderDto;
                            }
                            else
                            {
                                response.ReturnCode = 500;
                                response.Message = string.Format(ResponseMessages.GenericError, "Order", "updated");
                            }
                        }
                        else
                        {
                            response.ReturnCode = 500;
                            response.Message = string.Format(ResponseMessages.MappingNull, "Order");
                        }
                    }
                    else
                    {
                        response.ReturnCode = 404;
                        response.Message = string.Format(ResponseMessages.GetByIdNotFound, "Order", @event.Activity!.Id);
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
