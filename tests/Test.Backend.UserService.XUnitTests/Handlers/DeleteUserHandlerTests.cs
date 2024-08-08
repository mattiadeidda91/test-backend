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
    public class DeleteUserHandlerTests
    {
        private readonly Mock<IEventBusService> _mockMsgBus;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<DeleteUserStartedHandler>> _mockLogger;
        private readonly IOptions<KafkaOptions> _optionsKafka;
        private readonly DeleteUserStartedHandler _handler;

        public DeleteUserHandlerTests()
        {
            _mockMsgBus = new Mock<IEventBusService>();
            _mockUserService = new Mock<IUserService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<DeleteUserStartedHandler>>();
            _optionsKafka = Options.Create(new KafkaOptions { Producers = new Producers { ConsumerTopic = "response-topic" } });

            _handler = new DeleteUserStartedHandler(
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
            var @event = new DeleteUserStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new UserRequest { Id = Guid.NewGuid(), FirstName = "Mattia", LastName = "Test", Email = "test@example.com" }
            };
            var userDb = new User { Id = @event.Activity.Id, FirstName = "Mattia", LastName = "Test", Email = "test@example.com" };
            var userDto = new UserBaseDto { Id = userDb.Id, FirstName = "Mattia", LastName = "Test", Email = "test@example.com" };

            _mockUserService.Setup(s => s.GetByIdAsync(@event.Activity.Id)).ReturnsAsync(userDb);
            _mockUserService.Setup(s => s.DeleteAsync(@event.Activity.Id)).ReturnsAsync(true);
            _mockMapper.Setup(m => m.Map<UserBaseDto>(userDb)).Returns(userDto);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _mockMsgBus.Verify(m =>
                m.SendMessage(It.Is<DeleteUserResponse>(r => r.IsSuccess && r.Dto == userDto),
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
            var @event = new DeleteUserStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new UserRequest { Id = Guid.NewGuid(), FirstName = "Mattia", LastName = "Test", Email = "test@example.com" }
            };

            _mockUserService.Setup(s => s.GetByIdAsync(@event.Activity.Id)).ReturnsAsync((User)null);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _mockMsgBus.Verify(m => 
                m.SendMessage(It.Is<DeleteUserResponse>(r => !r.IsSuccess && r.Dto == null), 
                _optionsKafka.Value.Producers!.ConsumerTopic!, 
                It.IsAny<CancellationToken>(), 
                @event.CorrelationId, 
                null), 
                Times.Once);
        }


    }
}
