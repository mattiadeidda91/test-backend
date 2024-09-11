using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Product.Request;
using Test.Backend.Abstractions.Models.Dto.Product.Response;
using Test.Backend.Abstractions.Models.Events.Product;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;

namespace Test.Backend.WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> logger;
        private readonly KafkaOptions kafkaOptions;
        private readonly IEventBusService msgBus;

        public ProductController(IEventBusService msgBus, IOptions<KafkaOptions> kafkaOptions, ILogger<ProductController> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([Required] ProductRequest request, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<CreateProductStartedEvent, ProductRequest, CreateProductResponse>(
               request,
               kafkaOptions!.Consumers!.ProductTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status500InternalServerError, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<GetProductsStartedEvent, object, GetProductsResponse>(
               null,
               kafkaOptions!.Consumers!.ProductTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status404NotFound, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById([Required] Guid id, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<GetProductStartedEvent, ProductRequest, GetProductResponse>(
               new() { Id = id },
               kafkaOptions!.Consumers!.ProductTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status404NotFound, response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([Required] ProductRequest request, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<UpdateProductStartedEvent, ProductRequest, UpdateProductResponse>(
               request,
               kafkaOptions!.Consumers!.ProductTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status500InternalServerError, response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([Required] Guid id, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<DeleteProductStartedEvent, ProductRequest, DeleteProductResponse>(
               new() { Id = id},
               kafkaOptions!.Consumers!.ProductTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status500InternalServerError, response);
        }
    }
}
