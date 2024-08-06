﻿using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Events.User;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;

namespace Test.Backend.Services.UserService.Handlers
{
    public class DeleteUserStartedHandler : IEventHandler<DeleteUserStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;

        public DeleteUserStartedHandler(IEventBusService msgBus, IOptions<KafkaOptions> kafkaOptions)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
        }

        public Task HandleAsync(DeleteUserStartedEvent @event)
        {
            Console.WriteLine($"Handling DeleteUserStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

            msgBus.SendMessage(@event.Activity, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);

            return Task.CompletedTask;
        }
    }
}
