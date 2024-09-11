using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Order.Request;
using Test.Backend.Abstractions.Models.Dto.Order.Response;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;

namespace Test.Backend.WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> logger;
        private readonly KafkaOptions kafkaOptions;
        private readonly IEventBusService msgBus;

        public OrderController(IEventBusService msgBus, IOptions<KafkaOptions> kafkaOptions, ILogger<OrderController> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([Required] OrderRequest request, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<CreateOrderStartedEvent, OrderRequest, CreateOrderResponse>(
               request,
               kafkaOptions!.Consumers!.OrderTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status500InternalServerError, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<GetOrdersStartedEvent, object, GetOrdersResponse>(
               null,
               kafkaOptions!.Consumers!.OrderTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status404NotFound, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById([Required] Guid id, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<GetOrderStartedEvent, OrderRequest, GetOrderResponse>(
               new() { Id = id},
               kafkaOptions!.Consumers!.OrderTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status404NotFound, response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([Required] OrderRequest request, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<UpdateOrderStartedEvent, OrderRequest, UpdateOrderResponse>(
               request,
               kafkaOptions!.Consumers!.OrderTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status500InternalServerError, response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([Required] Guid id, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<DeleteOrderStartedEvent, OrderRequest, DeleteOrderResponse>(
               new() { Id = id},
               kafkaOptions!.Consumers!.OrderTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status500InternalServerError, response);
        }
    }
}
