using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.Product.Response;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Product;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Interfaces;
using Test.Backend.Services.ProductService.Service;

namespace Test.Backend.Services.ProductService.Handlers.Products
{
    public class UpdateProductStartedHandler : IEventHandler<UpdateProductStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly ICategoryService catagoryService;
        private readonly IProductService productService;
        private readonly IMapper mapper;
        private readonly ILogger<UpdateProductStartedHandler> logger;

        public UpdateProductStartedHandler(IEventBusService msgBus, IProductService productService, ICategoryService catagoryService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<UpdateProductStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.productService = productService;
            this.catagoryService = catagoryService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(UpdateProductStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<UpdateProductResponse, ProductWithoutOrderDto>(
               async () =>
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
                           var categoryDb = await catagoryService.GetByIdAsync(product.CategoryId);

                           if (categoryDb != null)
                           {
                               await productService.UpdateAsync(product);

                               response.IsSuccess = true;
                               response.Dto = mapper.Map<ProductWithoutOrderDto>(product);
                           }
                           else
                           {
                               response.ReturnCode = 404;
                               response.Messsage = string.Format(ResponseMessages.GetByIdNotFound, "Category", product.CategoryId);
                           }
                       }
                       else
                       {
                           response.ReturnCode = 500;
                           response.Messsage = string.Format(ResponseMessages.GenericError, "Product", "updated");
                       }
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
