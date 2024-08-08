using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Address.Response;
using Test.Backend.Abstractions.Models.Dto.User.Response;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Address;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.AddressService.Interfaces;

namespace Test.Backend.Services.AddressService.Handlers
{
    public class CreateAddressStartedHandler : IEventHandler<CreateAddressStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IAddressService addressService;
        private readonly IMapper mapper;
        private readonly ILogger<CreateAddressStartedHandler> logger;

        public CreateAddressStartedHandler(IEventBusService msgBus, IAddressService addressService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<CreateAddressStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.addressService = addressService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(CreateAddressStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<CreateAddressResponse, AddressBaseDto>(
               async () =>
               {
                   logger.LogInformation($"Handling CreateAddressStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   CreateAddressResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var address = mapper.Map<Address>(@event.Activity);
                   var alreadyExists = false;

                   if (address != null)
                   {
                       if (address.Id != Guid.Empty)
                       {
                           var addressDB = await addressService.GetByIdAsync(address.Id);
                           if (addressDB != null)
                           {
                               alreadyExists = true;
                               await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
                           }
                       }

                       if (!alreadyExists)
                       {
                           await addressService.SaveAsync(address);

                           response.IsSuccess = true;
                           response.Dto = mapper.Map<AddressBaseDto>(address);
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
