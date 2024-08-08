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
    public class CreateUserHandlerTests
    {
        private readonly Mock<IEventBusService> _mockMsgBus;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<CreateUserStartedHandler>> _mockLogger;
        private readonly IOptions<KafkaOptions> _optionsKafka;
        private readonly CreateUserStartedHandler _handler;

        public CreateUserHandlerTests()
        {
            _mockMsgBus = new Mock<IEventBusService>();
            _mockUserService = new Mock<IUserService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<CreateUserStartedHandler>>();
            _optionsKafka = Options.Create(new KafkaOptions { Producers = new Producers { ConsumerTopic = "response-topic" } });

            _handler = new CreateUserStartedHandler(
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
            var @event = new CreateUserStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new UserRequest { FirstName = "Mattia", LastName = "Test", Email = "mattia@example.com" }
            };
            var user = new User { Id = Guid.NewGuid(), FirstName = "Mattia", LastName = "Test", Email = "mattia@example.com" };
            var userDto = new UserBaseDto { Id = user.Id, FirstName = "Mattia", LastName = "Test", Email = "mattia@example.com" };

            _mockMapper.Setup(m => m.Map<User>(@event.Activity)).Returns(user);
            _mockUserService.Setup(s => s.SaveAsync(user)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserBaseDto>(user)).Returns(userDto);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _mockMsgBus.Verify(m =>
                m.SendMessage(It.Is<CreateUserResponse>(r => r.IsSuccess && r.Dto == userDto),
                "response-topic", It.IsAny<CancellationToken>(),
                @event.CorrelationId, null),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var @event = new CreateUserStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new UserRequest { FirstName = "Mattia", LastName = "Test", Email = "mattia@example.com" }
            };

            _mockMapper.Setup(m => m.Map<User>(@event.Activity)).Returns((User)null);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _mockMsgBus.Verify(m => m.SendMessage(It.Is<CreateUserResponse>(r => !r.IsSuccess && r.Dto == null), 
                "response-topic", 
                It.IsAny<CancellationToken>(), 
                @event.CorrelationId, null), 
                Times.Once);

            _mockUserService.Verify(s => s.SaveAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
