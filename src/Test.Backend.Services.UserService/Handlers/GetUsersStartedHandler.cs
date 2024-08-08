using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Dto.User.Response;
using Test.Backend.Abstractions.Models.Events.User;
using Test.Backend.Dependencies.Utils;
using Test.Backend.HtpClient.Interfaces;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.UserService.Interfaces;

namespace Test.Backend.Services.UserService.Handlers
{
    public class GetUsersStartedHandler : IEventHandler<GetUsersStartedEvent>
    {    
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly ILogger<GetUsersStartedHandler> logger;

        public GetUsersStartedHandler(IEventBusService msgBus, IUserService userService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<GetUsersStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.userService = userService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(GetUsersStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<GetUsersResponse, List<UserDto>>(
               async () =>
               {
                   logger.LogInformation($"Handling GetUsersStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   GetUsersResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var users = await userService.GetAsync();

                   if (users.Any())
                   {
                       response.IsSuccess = true;
                       response.Dto = mapper.Map<List<UserDto>>(users);
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
