using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.Product.Response;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Product;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.Services.ProductService.Handlers.Products
{
    public class UpdateProductStartedHandler : IEventHandler<UpdateProductStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IProductService productService;
        private readonly IMapper mapper;
        private readonly ILogger<UpdateProductStartedHandler> logger;

        public UpdateProductStartedHandler(IEventBusService msgBus, IProductService productService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<UpdateProductStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.productService = productService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(UpdateProductStartedEvent @event)
        {
            logger.LogInformation($"Handling UpdateProductStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

            UpdateProductResponse response = new()
            {
                IsSuccess = false,
                Dto = null
            };

            var productDb = await productService.GetByIdAsync(@event.Activity!.Id);

            if (productDb != null)
            {
                var product = mapper.Map<Product>(@event.Activity);

                if (product != null)
                {
                    await productService.UpdateAsync(product);

                    response.IsSuccess = true;
                    response.Dto = mapper.Map<ProductWithoutOrderDto>(product);
                }
            }

            await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);

        }
    }
}
