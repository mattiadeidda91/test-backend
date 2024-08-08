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
    public class UpdateUserHandlerTests
    {
        private readonly Mock<IEventBusService> _mockMsgBus;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<UpdateUserStartedHandler>> _mockLogger;
        private readonly IOptions<KafkaOptions> _optionsKafka;
        private readonly UpdateUserStartedHandler _handler;

        public UpdateUserHandlerTests()
        {
            _mockMsgBus = new Mock<IEventBusService>();
            _mockUserService = new Mock<IUserService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<UpdateUserStartedHandler>>();
            _optionsKafka = Options.Create(new KafkaOptions { Producers = new Producers { ConsumerTopic = "response-topic" } });

            _handler = new UpdateUserStartedHandler(
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
            var @event = new UpdateUserStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new UserRequest { Id = Guid.NewGuid(), FirstName = "UpdatedName" },
                CorrelationId = Guid.NewGuid().ToString()
            };

            var userDb = new User { Id = @event.Activity.Id, FirstName = "OldName" };
            var user = new User { Id = @event.Activity.Id, FirstName = "UpdatedName" };
            var userBaseDto = new UserBaseDto { Id = user.Id, FirstName = "UpdatedName" };

            _mockUserService.Setup(s => s.GetByIdAsync(@event.Activity.Id)).ReturnsAsync(userDb);
            _mockMapper.Setup(m => m.Map<User>(@event.Activity)).Returns(user);
            _mockMapper.Setup(m => m.Map<UserBaseDto>(user)).Returns(userBaseDto);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _mockUserService.Verify(s => s.UpdateAsync(user), Times.Once);
            _mockMsgBus.Verify(m => m.SendMessage(It.Is<UpdateUserResponse>(r => r.IsSuccess && r.Dto == userBaseDto), 
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
            var @event = new UpdateUserStartedEvent
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
            _mockUserService.Verify(s => s.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockMsgBus.Verify(m => m.SendMessage(It.Is<UpdateUserResponse>(r => !r.IsSuccess && r.Dto == null), 
                _optionsKafka.Value.Producers!.ConsumerTopic!, 
                It.IsAny<CancellationToken>(), 
                @event.CorrelationId, 
                null), 
                Times.Once);
        }


    }
}
