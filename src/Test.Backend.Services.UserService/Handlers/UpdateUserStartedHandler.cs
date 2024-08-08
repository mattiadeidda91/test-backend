using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Dto.User.Response;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.User;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.UserService.Interfaces;

namespace Test.Backend.Services.UserService.Handlers
{
    public class UpdateUserStartedHandler : IEventHandler<UpdateUserStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly ILogger<UpdateUserStartedHandler> logger;

        public UpdateUserStartedHandler(IEventBusService msgBus, IUserService userService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<UpdateUserStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.userService = userService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(UpdateUserStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<UpdateUserResponse, UserBaseDto>(
               async () =>
               {
                   logger.LogInformation($"Handling UpdateUserStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   UpdateUserResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var userDb = await userService.GetByIdAsync(@event.Activity!.Id);

                   if (userDb != null)
                   {
                       var user = mapper.Map<User>(@event.Activity);

                       if (user != null)
                       {
                           await userService.UpdateAsync(user);

                           response.IsSuccess = true;
                           response.Dto = mapper.Map<UserBaseDto>(user);
                       }
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
