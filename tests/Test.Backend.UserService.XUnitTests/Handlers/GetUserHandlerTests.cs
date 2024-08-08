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
using System.Diagnostics;

namespace Test.Backend.UserService.XUnitTests.Handlers
{
    public class GetUserHandlerTests
    {
        private readonly Mock<IEventBusService> _mockMsgBus;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<GetUserStartedHandler>> _mockLogger;
        private readonly IOptions<KafkaOptions> _optionsKafka;
        private readonly GetUserStartedHandler _handler;

        public GetUserHandlerTests()
        {
            _mockMsgBus = new Mock<IEventBusService>();
            _mockUserService = new Mock<IUserService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<GetUserStartedHandler>>();
            _optionsKafka = Options.Create(new KafkaOptions { Producers = new Producers { ConsumerTopic = "response-topic" } });

            _handler = new GetUserStartedHandler(
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
            var @event = new GetUserStartedEvent
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

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Matt",
                LastName = "Test",
                Email = "test@example.com"
            };

            var userDto = new UserDto
            {
                Id = Guid.NewGuid(),
                FirstName = "Matt",
                LastName = "Test",
                Email = "test@example.com"
            };

            _mockUserService.Setup(s => s.GetByIdAsync(@event.Activity.Id)).ReturnsAsync(user);
            _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _mockMsgBus.Verify(m => m.SendMessage(It.Is<GetUserResponse>(r => r.IsSuccess && r.Dto == userDto),
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
            var @event = new GetUserStartedEvent
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

            _mockUserService.Setup(s => s.GetByIdAsync(@event.Activity.Id)).ReturnsAsync((User)null);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _mockMsgBus.Verify(m => m.SendMessage(It.Is<GetUserResponse>(r => !r.IsSuccess && r.Dto == null), 
                _optionsKafka.Value.Producers!.ConsumerTopic!, 
                It.IsAny<CancellationToken>(), 
                @event.CorrelationId, null), 
                Times.Once);
        }


    }
}
