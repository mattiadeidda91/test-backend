﻿using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Address.Response;
using Test.Backend.Abstractions.Models.Dto.Address;
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
    public class CreateCategoryStartedHandler : IEventHandler<CreateCategoryStartedEvent>
    {
        private readonly IEventBusService msgBus;
        private readonly KafkaOptions kafkaOptions;
        private readonly ICategoryService categoryService;
        private readonly IMapper mapper;
        private readonly ILogger<CreateCategoryStartedHandler> logger;

        public CreateCategoryStartedHandler(IEventBusService msgBus, ICategoryService categoryService, IMapper mapper, IOptions<KafkaOptions> kafkaOptions, ILogger<CreateCategoryStartedHandler> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.categoryService = categoryService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task HandleAsync(CreateCategoryStartedEvent @event)
        {
            await HandlerExceptionUtility.HandleExceptionsAsync<CreateCategoryResponse, CategoryBaseDto>(
               async () =>
               {
                   logger.LogInformation($"Handling CreateCategoryStartedEvent: {@event.ActivityId}, {JsonSerializer.Serialize(@event.Activity)}");

                   CreateCategoryResponse response = new()
                   {
                       IsSuccess = false,
                       Dto = null
                   };

                   var category = mapper.Map<Category>(@event.Activity);
                   var alreadyExists = false;

                   if (category != null)
                   {
                       if (category.Id != Guid.Empty)
                       {
                           var categoryDB = await categoryService.GetByIdAsync(category.Id);
                           if (categoryDB != null)
                           {
                               alreadyExists = true;
                               await msgBus.SendMessage(response, kafkaOptions.Producers!.ConsumerTopic!, new CancellationToken(), @event.CorrelationId, null);
                           }
                       }

                       if (!alreadyExists)
                       {
                           await categoryService.SaveAsync(category);

                           response.IsSuccess = true;
                           response.Dto = mapper.Map<CategoryBaseDto>(category);
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
