using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.Product.Response;
using Test.Backend.Abstractions.Models.Events.Product;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.Services.ProductService.Handlers.Products
{
    public class GetProductStartedHandler : IEventHandler<GetProductStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IProductService productService;
        private readonly IMapper mapper;
        private readonly ILogger<GetProductStartedHandler> logger;

        public GetProductStartedHandler(IEventBusService msgBus, IProductService productService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<GetProductStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.productService = productService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(GetProductStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<GetProductResponse, ProductDto>(
               async () =>
               {
                   logger.LogInformation($"Handling GetProductStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   GetProductResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var product = await productService.GetByIdAsync(@event.Activity!.Id);

                   if (product != null)
                   {
                       response.IsSuccess = true;
                       response.Dto = mapper.Map<ProductDto>(product);
                   }
                   else
                   {
                       response.ReturnCode = 404;
                       response.Messsage = string.Format(ResponseMessages.GetByIdNotFound, "Product", @event.Activity!.Id);
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
