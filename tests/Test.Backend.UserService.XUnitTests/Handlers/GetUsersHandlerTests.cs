using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.User.Request;
using Test.Backend.Abstractions.Models.Dto.User.Response;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.User;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.UserService.Handlers;
using Test.Backend.Services.UserService.Interfaces;

namespace Test.Backend.UserService.XUnitTests.Handlers
{
    public class GetUsersHandlerTests
    {
        private readonly Mock<IEventBusService> _mockMsgBus;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<GetUsersStartedHandler>> _mockLogger;
        private readonly IOptions<KafkaOptions> _optionsKafka;
        private readonly GetUsersStartedHandler _handler;

        public GetUsersHandlerTests()
        {
            _mockMsgBus = new Mock<IEventBusService>();
            _mockUserService = new Mock<IUserService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<GetUsersStartedHandler>>();
            _optionsKafka = Options.Create(new KafkaOptions { Producers = new Producers { ConsumerTopic = "response-topic" } });

            _handler = new GetUsersStartedHandler(
                _mockMsgBus.Object,
                _mockUserService.Object,
                _mockMapper.Object,
                _optionsKafka,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task HandleAsync_Successful()
        {
            // Arrange
            var @event = new GetUsersStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new UserRequest()
                {
                    Email = "test@example.com",
                    FirstName = "Matt",
                    LastName = "Test",
                    Id = Guid.NewGuid()
                },
                CorrelationId = "correlation-123"
            };

            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), FirstName = "Matt", LastName = "Test", Email = "test@example.com" },
                new User { Id = Guid.NewGuid(), FirstName = "Carlo", LastName = "Pino", Email = "carlo@example.com" }
            };

            var usersDto = new List<UserDto>
            {
                new UserDto { Id = users[0].Id, FirstName = "Matt", LastName = "Test", Email = "test@example.com" },
                new UserDto { Id = users[1].Id, FirstName = "Carlo", LastName = "Pino", Email = "carlo@example.com" }
            };

            _mockUserService.Setup(s => s.GetAsync()).ReturnsAsync(users);
            _mockMapper.Setup(m => m.Map<List<UserDto>>(users)).Returns(usersDto);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _mockMsgBus.Verify(m => m.SendMessage(It.Is<GetUsersResponse>(r => r.IsSuccess && r.Dto == usersDto),
                _optionsKafka.Value.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(),
                @event.CorrelationId,
                null),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var @event = new GetUsersStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new UserRequest()
                {
                    Email= "test@example.com",
                    FirstName= "Matt",
                    LastName= "Test",
                    Id = Guid.NewGuid()
                },
                CorrelationId = "correlation-123"
            };

            _mockUserService.Setup(s => s.GetAsync()).ReturnsAsync(new List<User>());

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _mockMsgBus.Verify(m => m.SendMessage(It.Is<GetUsersResponse>(r => !r.IsSuccess && r.Dto == null),
                _optionsKafka.Value.Producers!.ConsumerTopic!,
                It.IsAny<CancellationToken>(), 
                @event.CorrelationId, 
                null), 
                Times.Once);
        }
    }
}
