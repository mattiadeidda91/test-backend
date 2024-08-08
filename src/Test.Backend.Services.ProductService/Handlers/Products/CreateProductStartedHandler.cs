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
    public class CreateProductStartedHandler : IEventHandler<CreateProductStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly IProductService productService;
        private readonly ICategoryService catagoryService;
        private readonly IMapper mapper;
        private readonly ILogger<CreateProductStartedHandler> logger;

        public CreateProductStartedHandler(IEventBusService msgBus, IProductService productService, ICategoryService catagoryService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<CreateProductStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.productService = productService;
            this.catagoryService= catagoryService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(CreateProductStartedEvent @event)
        {
            logger.LogInformation($"Handling CreateProductStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

            CreateProductResponse response = new()
            {
                IsSuccess = false,
                Dto = null
            };

            var product = mapper.Map<Product>(@event.Activity);
            var alreadyExists = false;

            if (product != null)
            {
                if (product.Id != Guid.Empty)
                {
                    var productDB = await productService.GetByIdAsync(product.Id);
                    if (productDB != null)
                    {
                        alreadyExists = true;
                        await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
                    }
                }

                var categoryDb = await catagoryService.GetByIdAsync(product.CategoryId);

                if (!alreadyExists && categoryDb != null)
                {
                    await productService.SaveAsync(product);

                    response.IsSuccess = true;
                    response.Dto = mapper.Map<ProductWithoutOrderDto>(product);
                }
            }

            await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
        }
    }
}
