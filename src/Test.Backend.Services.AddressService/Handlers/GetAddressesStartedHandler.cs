using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Address.Response;
using Test.Backend.Abstractions.Models.Events.Address;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.AddressService.Interfaces;

namespace Test.Backend.Services.UserService.Handlers
{
    public class GetAddressesStartedHandler : IEventHandler<GetAddressesStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IAddressService addressService;
        private readonly IMapper mapper;
        private readonly ILogger<GetAddressesStartedHandler> logger;

        public GetAddressesStartedHandler(IEventBusService msgBus, IAddressService addressService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<GetAddressesStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.addressService = addressService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(GetAddressesStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<GetAddressesResponse, List<AddressDto>>(
               async () =>
               {
                   logger.LogInformation($"Handling GetAddressesStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   GetAddressesResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var addresses = await addressService.GetAsync();

                   if (addresses.Any())
                   {
                       response.IsSuccess = true;
                       response.Dto = mapper.Map<List<AddressDto>>(addresses);
                   }
                   else
                   {
                       response.ReturnCode = 404;
                       response.Messsage = string.Format(ResponseMessages.GetNotFound, "Addresses");
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
