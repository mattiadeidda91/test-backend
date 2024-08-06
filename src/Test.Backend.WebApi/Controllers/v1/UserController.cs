using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Dto.User.Request;
using Test.Backend.Abstractions.Models.Dto.User.Response;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.User;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.UserService.Interfaces;

namespace Test.Backend.WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly IMapper mapper;
        private readonly IUserService userService;

        private readonly KafkaOptions kafkaOptions;
        private readonly IEventBusService msgBus;

        public UserController(IEventBusService msgBus, IOptions<KafkaOptions> kafkaOptions, IUserService userService, ILogger<UserController> logger, IMapper mapper)
        {
            this.msgBus = msgBus;
            this.kafkaOptions = kafkaOptions.Value;

            this.userService = userService;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([Required] UserRequest request, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid().ToString();

            (var message, var headers) = msgBus.GenerateMsgBusEvent<CreateUserStartedEvent, UserRequest>(request, correlationId);

            _ = await msgBus.SendMessage(message, kafkaOptions!.Consumers!.UserTopic!, cancellationToken, correlationId, headers);

            var response = await msgBus.ConsumeAsync<CreateUserResponse>("response-topic", correlationId);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response?.Dto);

            //var user = mapper.Map<User>(request);

            //if (user != null)
            //{
            //    await userService.SaveAsync(user);

            //    return Ok();
            //}
            //else
            //    return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid().ToString();

            (var message, var headers) = msgBus.GenerateMsgBusEvent<GetUsersStartedEvent, object>(null, correlationId);

            _ = await msgBus.SendMessage(message, kafkaOptions!.Consumers!.UserTopic!, cancellationToken, correlationId, headers);

            var response = await msgBus.ConsumeAsync<GetUsersResponse>("response-topic", correlationId);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response?.Dto);

            //var users = await userService.GetAsync();

            //var usersDto = mapper.Map<List<UserDto>>(users);

            //return StatusCode(usersDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, usersDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([Required] Guid id, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid().ToString();

            (var message, var headers) = msgBus.GenerateMsgBusEvent<GetUserStartedEvent, UserRequest>(new() { Id = id}, correlationId);

            _ = await msgBus.SendMessage(message, kafkaOptions!.Consumers!.UserTopic!, cancellationToken, correlationId, headers);

            var response = await msgBus.ConsumeAsync<GetUserResponse>("response-topic", correlationId);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response?.Dto);

            //var users = await userService.GetByIdAsync(id);

            //var userDto = mapper.Map<UserDto>(users);

            //return StatusCode(userDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, userDto);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([Required] UserRequest request, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid().ToString();

            (var message, var headers) = msgBus.GenerateMsgBusEvent<UpdateUserStartedEvent, UserRequest>(request, correlationId);

            _ = await msgBus.SendMessage(message, kafkaOptions!.Consumers!.UserTopic!, cancellationToken, correlationId, headers);

            var response = await msgBus.ConsumeAsync<UpdateUserResponse>("response-topic", correlationId);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response?.Dto);

            //var userDb = userService.GetByIdAsync(request.Id);

            //if (userDb != null)
            //{
            //    var user = mapper.Map<User>(request);

            //    if (user != null)
            //    {
            //        await userService.UpdateAsync(user);

            //        return Ok();
            //    }
            //    else
            //        return BadRequest();
            //}
            //else
            //{
            //    return NotFound();
            //}
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([Required] Guid id, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid().ToString();

            (var message, var headers) = msgBus.GenerateMsgBusEvent<DeleteUserStartedEvent, UserRequest>(new() { Id = id}, correlationId);

            _ = await msgBus.SendMessage(message, kafkaOptions!.Consumers!.UserTopic!, cancellationToken, correlationId, headers);

            var response = await msgBus.ConsumeAsync<DeleteUserResponse>("response-topic", correlationId);

            return StatusCode(response?.IsSuccess ?? false ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, response);

            //var isDeleted = await userService.DeleteAsync(id);

            //return StatusCode(isDeleted ? StatusCodes.Status200OK : StatusCodes.Status404NotFound);
        }
    }
}
