﻿using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Dto.User.Response;
using Test.Backend.Abstractions.Models.Events.User;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.UserService.Interfaces;

namespace Test.Backend.Services.UserService.Handlers
{
    public class GetUserStartedHandler : IEventHandler<GetUserStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly ILogger<GetUserStartedHandler> logger;

        public GetUserStartedHandler(IEventBusService msgBus, IUserService userService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<GetUserStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.userService = userService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(GetUserStartedEvent @event)
        {
           await HandlerExceptionUtility.HandleExceptionsAsync<GetUserResponse, UserDto>(
                async () =>
                {
                    logger.LogInformation($"Handling GetUserStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                    GetUserResponse response = new()
                    {
                        IsSuccess = false,
                        Dto = null
                    };

                    var user = await userService.GetByIdAsync(@event.Activity!.Id);

                    if(user != null)
                    {
                        response.IsSuccess = true;
                        response.Dto = mapper.Map<UserDto>(user);
                    }
                    else
                    {
                        response.ReturnCode = 404;
                        response.Message = string.Format(ResponseMessages.GetByIdNotFound, "User", @event.Activity!.Id);
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
