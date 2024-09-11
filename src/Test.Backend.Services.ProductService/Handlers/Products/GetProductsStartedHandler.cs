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
    public class GetProductsStartedHandler : IEventHandler<GetProductsStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IProductService productService;
        private readonly IMapper mapper;
        private readonly ILogger<GetProductsStartedHandler> logger;

        public GetProductsStartedHandler(IEventBusService msgBus, IProductService productService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<GetProductsStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.productService = productService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(GetProductsStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<GetProductsResponse, List<ProductDto>>(
               async () =>
               {
                   logger.LogInformation($"Handling GetProductsStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   GetProductsResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var products = await productService.GetAsync();

                   if (products.Any())
                   {
                       response.IsSuccess = true;
                       response.Dto = mapper.Map<List<ProductDto>>(products);
                   }
                   else
                   {
                       response.ReturnCode = 404;
                       response.Messsage = string.Format(ResponseMessages.GetNotFound, "Produts");
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
