using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Dto.Category.Response;
using Test.Backend.Abstractions.Models.Events.Category;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.Services.ProductService.Handlers.Categories
{
    public class DeleteCategoryStartedHandler : IEventHandler<DeleteCategoryStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly ICategoryService categoryService;
        private readonly IMapper mapper;
        private readonly ILogger<DeleteCategoryStartedHandler> logger;

        public DeleteCategoryStartedHandler(IEventBusService msgBus, ICategoryService categoryService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<DeleteCategoryStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.categoryService = categoryService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(DeleteCategoryStartedEvent @event)
        {
            logger.LogInformation($"Handling DeleteCategoryStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

            DeleteCategoryResponse response = new()
            {
                IsSuccess = false,
                Dto = null
            };

            var categoryDb = await categoryService.GetByIdAsync(@event.Activity!.Id);

            if (categoryDb != null)
            {
                var isDeleted = await categoryService.DeleteAsync(@event.Activity!.Id);

                if (isDeleted)
                {
                    response.IsSuccess = true;
                    response.Dto = mapper.Map<CategoryBaseDto>(categoryDb);
                }
            }

            await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
        }
    }
}
