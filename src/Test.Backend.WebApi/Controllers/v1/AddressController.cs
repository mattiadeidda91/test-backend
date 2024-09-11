using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Address.Request;
using Test.Backend.Abstractions.Models.Dto.Address.Response;
using Test.Backend.Abstractions.Models.Events.Address;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;

namespace Test.Backend.WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly ILogger<AddressController> logger;
        private readonly KafkaOptions kafkaOptions;
        private readonly IEventBusService msgBus;

        public AddressController(IEventBusService msgBus, IOptions<KafkaOptions> kafkaOptions, ILogger<AddressController> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress([Required] AddressRequest request, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<CreateAddressStartedEvent, AddressRequest, CreateAddressResponse>(
               request,
               kafkaOptions!.Consumers!.AddressTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status500InternalServerError, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAddresses(CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<GetAddressesStartedEvent, object, GetAddressesResponse>(
               null,
               kafkaOptions!.Consumers!.AddressTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status404NotFound, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressById([Required] Guid id, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<GetAddressStartedEvent, AddressRequest, GetAddressResponse>(
               new() { Id = id },
               kafkaOptions!.Consumers!.AddressTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status404NotFound, response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAddress([Required] AddressRequest request, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<UpdateAddressStartedEvent, AddressRequest, UpdateAddressResponse>(
               request,
               kafkaOptions!.Consumers!.AddressTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status500InternalServerError, response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAddress([Required] Guid id, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<DeleteAddressStartedEvent, AddressRequest, DeleteAddressResponse>(
               new() { Id = id },
               kafkaOptions!.Consumers!.AddressTopic!,
               kafkaOptions!.Producers!.ConsumerTopic!,
               cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : response?.ReturnCode ?? StatusCodes.Status500InternalServerError, response);
        }
    }
}
