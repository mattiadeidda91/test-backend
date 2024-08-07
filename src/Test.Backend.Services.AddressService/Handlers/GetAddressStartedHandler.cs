using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Address.Response;
using Test.Backend.Abstractions.Models.Events.Address;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.AddressService.Interfaces;

namespace Test.Backend.Services.UserService.Handlers
{
    public class GetAddressStartedHandler : IEventHandler<GetAddressStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IAddressService addressService;
        private readonly IMapper mapper;
        private readonly ILogger<GetAddressStartedHandler> logger;

        public GetAddressStartedHandler(IEventBusService msgBus, IAddressService addressService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<GetAddressStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.addressService = addressService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(GetAddressStartedEvent @event)
        {
            logger.LogInformation($"Handling GetAddressStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

            GetAddressResponse response = new()
            {
                IsSuccess = false,
                Dto = null
            };

            var address = await addressService.GetByIdAsync(@event.Activity!.Id);

            if(address != null)
            {
                response.IsSuccess = true;
                response.Dto = mapper.Map<AddressDto>(address);
            }

            //TODO: implement call to OrderService to retrieve Orders for users

            await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
        }
    }
}
