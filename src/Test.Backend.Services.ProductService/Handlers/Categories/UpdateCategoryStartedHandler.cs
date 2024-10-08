﻿using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Dto.Category.Response;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Category;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.Services.ProductService.Handlers.Categories
{
    public class UpdateCategoryStartedHandler : IEventHandler<UpdateCategoryStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly ICategoryService categoryService;
        private readonly IMapper mapper;
        private readonly ILogger<UpdateCategoryStartedHandler> logger;

        public UpdateCategoryStartedHandler(IEventBusService msgBus, ICategoryService categoryService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<UpdateCategoryStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.categoryService = categoryService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(UpdateCategoryStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<UpdateCategoryResponse, CategoryBaseDto>(
               async () =>
               {
                   logger.LogInformation($"Handling UpdateCategoryStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   UpdateCategoryResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var productDb = await categoryService.GetByIdAsync(@event.Activity!.Id);

                   if (productDb != null)
                   {
                       var category = mapper.Map<Category>(@event.Activity);

                       if (category != null)
                       {
                           await categoryService.UpdateAsync(category);

                           response.IsSuccess = true;
                           response.Dto = mapper.Map<CategoryBaseDto>(category);
                       }
                       else
                       {
                           response.ReturnCode = 500;
                           response.Message = string.Format(ResponseMessages.GenericError, "Category", "updated");
                       }
                   }
                   else
                   {
                       response.ReturnCode = 404;
                       response.Message = string.Format(ResponseMessages.GetByIdNotFound, "Category", @event.Activity!.Id);
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
