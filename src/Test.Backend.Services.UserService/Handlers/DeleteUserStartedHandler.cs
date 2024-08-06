using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Dto.User.Response;
using Test.Backend.Abstractions.Models.Events.User;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.UserService.Interfaces;

namespace Test.Backend.Services.UserService.Handlers
{
    public class DeleteUserStartedHandler : IEventHandler<DeleteUserStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly ILogger<DeleteUserStartedHandler> logger;

        public DeleteUserStartedHandler(IEventBusService msgBus, IUserService userService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<DeleteUserStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.userService = userService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(DeleteUserStartedEvent @event)
        {
            logger.LogInformation($"Handling DeleteUserStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

            DeleteUserResponse response = new()
            {
                IsSuccess = false,
                Dto = null
            };

            var userDb = await userService.GetByIdAsync(@event.Activity!.Id);

            if (userDb != null)
            {
                var isDeleted = await userService.DeleteAsync(@event.Activity!.Id);

                if (isDeleted)
                {
                    response.IsSuccess = true;
                    response.Dto = mapper.Map<UserDto>(userDb);
                }
            }

            await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
        }
    }
}
