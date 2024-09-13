using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Address.Response;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Address;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.AddressService.Interfaces;

namespace Test.Backend.Services.UserService.Handlers
{
    public class UpdateAddressStartedHandler : IEventHandler<UpdateAddressStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IAddressService addressService;
        private readonly IMapper mapper;
        private readonly ILogger<UpdateAddressStartedHandler> logger;

        public UpdateAddressStartedHandler(IEventBusService msgBus, IAddressService addressService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<UpdateAddressStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.addressService = addressService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(UpdateAddressStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<UpdateAddressResponse, AddressBaseDto>(
               async () =>
               {
                   logger.LogInformation($"Handling UpdateAddressStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   UpdateAddressResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var addressDb = await addressService.GetByIdAsync(@event.Activity!.Id);

                   if (addressDb != null)
                   {
                       var address = mapper.Map<Address>(@event.Activity);

                       if (address != null)
                       {
                           await addressService.UpdateAsync(address);

                           response.IsSuccess = true;
                           response.Dto = mapper.Map<AddressBaseDto>(address);
                       }
                       else
                       {
                           response.ReturnCode = 500;
                           response.Message = string.Format(ResponseMessages.GenericError, "Address", "updated");
                       }
                   }
                   else
                   {
                       response.ReturnCode = 404;
                       response.Message = string.Format(ResponseMessages.GetByIdNotFound, "Address", @event.Activity!.Id);
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
