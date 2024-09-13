using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Dto.Category.Response;
using Test.Backend.Abstractions.Models.Events.Category;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.Services.ProductService.Handlers.Categories
{
    public class GetCategoriesStartedHandler : IEventHandler<GetCategoriesStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly ICategoryService categoryService;
        private readonly IMapper mapper;
        private readonly ILogger<GetCategoriesStartedHandler> logger;

        public GetCategoriesStartedHandler(IEventBusService msgBus, ICategoryService categoryService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<GetCategoriesStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.categoryService = categoryService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(GetCategoriesStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<GetCategoriesResponse, List<CategoryDto>>(
               async () =>
               {
                   logger.LogInformation($"Handling GetCategoriesStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   GetCategoriesResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var categories = await categoryService.GetAsync();

                   if (categories.Any())
                   {
                       response.IsSuccess = true;
                       response.Dto = mapper.Map<List<CategoryDto>>(categories);
                   }
                   else
                   {
                       response.ReturnCode = 404;
                       response.Message = string.Format(ResponseMessages.GetNotFound, "Categories");
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
