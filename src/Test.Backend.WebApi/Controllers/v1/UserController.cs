using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.User.Request;
using Test.Backend.Abstractions.Models.Dto.User.Response;
using Test.Backend.Abstractions.Models.Events.User;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;

namespace Test.Backend.WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly KafkaOptions kafkaOptions;
        private readonly IEventBusService msgBus;

        public UserController(IEventBusService msgBus, IOptions<KafkaOptions> kafkaOptions, ILogger<UserController> logger)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([Required] UserRequest request, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<CreateUserStartedEvent, UserRequest, CreateUserResponse>(
                request,
                kafkaOptions!.Consumers!.UserTopic!,
                kafkaOptions!.Producers!.ConsumerTopic!,
                cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response?.Dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<GetUsersStartedEvent, object, GetUsersResponse>(
                null,
                kafkaOptions!.Consumers!.UserTopic!,
                kafkaOptions!.Producers!.ConsumerTopic!,
                cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response?.Dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([Required] Guid id, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<GetUserStartedEvent, UserRequest, GetUserResponse>(
                new() { Id = id },
                kafkaOptions!.Consumers!.UserTopic!,
                kafkaOptions!.Producers!.ConsumerTopic!,
                cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response?.Dto);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([Required] UserRequest request, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<UpdateUserStartedEvent, UserRequest, UpdateUserResponse>(
                request,
                kafkaOptions!.Consumers!.UserTopic!,
                kafkaOptions!.Producers!.ConsumerTopic!,
                cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response?.Dto);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([Required] Guid id, CancellationToken cancellationToken)
        {
            var response = await msgBus.HandleMsgBusMessages<DeleteUserStartedEvent, UserRequest, DeleteUserResponse>(
                new() { Id = id },
                kafkaOptions!.Consumers!.UserTopic!,
                kafkaOptions!.Producers!.ConsumerTopic!,
                cancellationToken);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response);
        }
    }
}
