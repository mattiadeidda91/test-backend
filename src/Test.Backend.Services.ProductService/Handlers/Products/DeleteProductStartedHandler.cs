using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
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
    public class DeleteProductStartedHandler : IEventHandler<DeleteProductStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IProductService productService;
        private readonly IMapper mapper;
        private readonly ILogger<DeleteProductStartedHandler> logger;

        public DeleteProductStartedHandler(IEventBusService msgBus, IProductService productService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<DeleteProductStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.productService = productService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(DeleteProductStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<DeleteProductResponse, ProductWithoutOrderDto>(
               async () =>
               {
                   logger.LogInformation($"Handling DeleteProductStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   DeleteProductResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var productDb = await productService.GetByIdAsync(@event.Activity!.Id);

                   if (productDb != null)
                   {
                       var isDeleted = await productService.DeleteAsync(@event.Activity!.Id);

                       if (isDeleted)
                       {
                           response.IsSuccess = true;
                           response.Dto = mapper.Map<ProductWithoutOrderDto>(productDb);
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
