using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Category.Request;
using Test.Backend.Abstractions.Models.Dto.Category.Response;
using Test.Backend.Abstractions.Models.Events.Category;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;

namespace Test.Backend.WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ILogger<CategoryController> logger;
        private readonly KafkaOptions kafkaOptions;
        private readonly IEventBusService msgBus;

        public CategoryController(IEventBusService msgBus, IOptions<KafkaOptions> kafkaOptions, ILogger<CategoryController> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress([Required] CategoryRequest request, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<CreateCategoryStartedEvent, CategoryRequest, CreateCategoryResponse>(
               request,
               kafkaOptions!.Consumers!.ProductTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest, response?.Dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<GetCategoriesStartedEvent, object, GetCategoriesResponse>(
                null,
                kafkaOptions!.Consumers!.ProductTopic!,
                kafkaOptions!.Producers!.ConsumerTopic!,
                cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response?.Dto);       
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById([Required] Guid id, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<GetCategoryStartedEvent, CategoryRequest, GetCategoryResponse>(
                new() { Id = id },
                kafkaOptions!.Consumers!.ProductTopic!,
                kafkaOptions!.Producers!.ConsumerTopic!,
                cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response?.Dto);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([Required] CategoryRequest request, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<UpdateCategoryStartedEvent, CategoryRequest, UpdateCategoryResponse>(
                request,
                kafkaOptions!.Consumers!.ProductTopic!,
                kafkaOptions!.Producers!.ConsumerTopic!,
                cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response?.Dto);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([Required] Guid id, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<DeleteCategoryStartedEvent, CategoryRequest, DeleteCategoryResponse>(
                new() { Id = id },
                kafkaOptions!.Consumers!.ProductTopic!,
                kafkaOptions!.Producers!.ConsumerTopic!,
                cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response);
        }
    }
}
