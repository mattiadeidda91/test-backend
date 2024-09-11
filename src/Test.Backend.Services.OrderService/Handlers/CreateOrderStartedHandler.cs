using AutoMapper;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.Dependencies.Utils;
using Test.Backend.HtpClient.Interfaces;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.OrderService.Interfaces;

namespace Test.Backend.Services.OrderService.Handlers
{
    public class CreateOrderStartedHandler : IEventHandler<CreateOrderStartedEvent>
    {
        private readonly IOrderEntitesService orderEntitesService;
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;
        private readonly ILogger<CreateOrderStartedHandler> logger;

        public CreateOrderStartedHandler(IEventBusService msgBus, IOrderService orderService, IOrderEntitesService orderEntitesService,
            IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<CreateOrderStartedHandler> logger)
        {
            this.orderEntitesService= orderEntitesService;
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.orderService = orderService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(CreateOrderStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<CreateOrderResponse, OrderDto>(
                async () =>
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

                                response.ReturnCode = 409;
                                response.Messsage = string.Format(ResponseMessages.Conflict, "Order", order.Id);

                                await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
                            }
                        }
                        else
                        {
                            response.ReturnCode = 400;
                            response.Messsage = string.Format(ResponseMessages.GuidEmpty, "Order");
                        }

                        if (!alreadyExists)
                        {
                            (var orderCanCreate, var userDto, var addressDto, var productsDto) = await orderEntitesService.CheckAndGetExistingEntities(order.UserId, order.DeliveryAddressId, @event.Activity?.ProductIds?.ToList() ?? new List<Guid>());

                            if (orderCanCreate)
                            {
                                await orderService.SaveAsync(order);

                                var orderDto = mapper.Map<OrderDto>(order);
                                orderDto.User = userDto;
                                orderDto.Address = addressDto;
                                orderDto.Products = productsDto;

                                response.IsSuccess = true;
                                response.Dto = orderDto;
                                response.ReturnCode = 200;
                                response.Messsage = string.Format(ResponseMessages.CreatedSuccessfull, "Order");
                            }
                        }
                    }
                    else
                    {
                        response.ReturnCode = 500;
                        response.Messsage = string.Format(ResponseMessages.MappingNull, "Order");
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
