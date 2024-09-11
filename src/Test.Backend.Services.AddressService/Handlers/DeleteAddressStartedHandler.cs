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

namespace Test.Backend.Services.AddressService.Handlers
{
    public class DeleteAddressStartedHandler : IEventHandler<DeleteAddressStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IAddressService addressService;
        private readonly IMapper mapper;
        private readonly ILogger<DeleteAddressStartedHandler> logger;

        public DeleteAddressStartedHandler(IEventBusService msgBus, IAddressService addressService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<DeleteAddressStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.addressService = addressService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(DeleteAddressStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<DeleteAddressResponse, AddressBaseDto>(
               async () =>
               {
                   logger.LogInformation($"Handling DeleteAddressStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   DeleteAddressResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var addressDb = await addressService.GetByIdAsync(@event.Activity!.Id);

                   if (addressDb != null)
                   {
                       var isDeleted = await addressService.DeleteAsync(@event.Activity!.Id);

                       if (isDeleted)
                       {
                           response.IsSuccess = true;
                           response.Dto = mapper.Map<AddressBaseDto>(addressDb);
                       }
                       else
                       {
                           response.ReturnCode = 500;
                           response.Messsage = string.Format(ResponseMessages.GenericError, "Address", "deleted");
                       }
                   }
                   else
                   {
                       response.ReturnCode = 404;
                       response.Messsage = string.Format(ResponseMessages.GetByIdNotFound, "Address", @event.Activity!.Id);
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
