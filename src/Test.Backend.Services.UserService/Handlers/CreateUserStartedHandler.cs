using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Product.Response;
using Test.Backend.Abstractions.Models.Dto.Product;
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
    public class CreateUserStartedHandler : IEventHandler<CreateUserStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly ILogger<CreateUserStartedHandler> logger;

        public CreateUserStartedHandler(IEventBusService msgBus, IUserService userService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<CreateUserStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.userService = userService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(CreateUserStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<CreateUserResponse, UserBaseDto>(
               async () =>
               {
                   logger.LogInformation($"Handling CreateUserStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   CreateUserResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var user = mapper.Map<User>(@event.Activity);
                   var alreadyExists = false;

                   if (user != null)
                   {
                       if (user.Id != Guid.Empty)
                       {
                           var userDB = await userService.GetByIdAsync(user.Id);
                           if (userDB != null)
                           {
                               alreadyExists = true;
                               await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
                           }
                       }

                       if (!alreadyExists)
                       {
                           await userService.SaveAsync(user);

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
