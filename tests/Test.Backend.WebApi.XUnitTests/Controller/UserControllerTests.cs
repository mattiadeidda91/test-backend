using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Dto.User.Request;
using Test.Backend.Abstractions.Models.Dto.User.Response;
using Test.Backend.Abstractions.Models.Events.User;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.WebApi.Controllers.v1;

namespace Test.Backend.WebApi.XUnitTests.Controller
{
    public class UserControllerTests
    {
        private readonly Mock<IEventBusService> _mockEventBusService;
        private readonly Mock<IOptions<KafkaOptions>> _mockKafkaOptions;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly KafkaOptions _kafkaOptions;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockEventBusService = new Mock<IEventBusService>();
            _mockKafkaOptions = new Mock<IOptions<KafkaOptions>>();
            _mockLogger = new Mock<ILogger<UserController>>();

            _kafkaOptions = new KafkaOptions
            {
                Consumers = new Consumers
                {
                    AddressTopic = "user-topic"
                },
                Producers = new Producers
                {
                    ConsumerTopic = "consumer-topic"
                }
            };

            _mockKafkaOptions.Setup(opt => opt.Value).Returns(_kafkaOptions);

            _controller = new UserController(_mockEventBusService.Object, _mockKafkaOptions.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateUser_Successful()
        {
            // Arrange
            var userDto = new UserBaseDto { FirstName = "Matt", LastName = "Test", Email = "matt.test@example.com" };
            var response = new CreateUserResponse { IsSuccess = true, Dto = userDto };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<CreateUserStartedEvent, UserRequest, CreateUserResponse>(
                    It.IsAny<UserRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateUser(new() { FirstName = "Matt", LastName = "Test", Email = "matt.test@example.com" }, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response.Dto, okResult.Value);
        }

        [Fact]
        public async Task CreateUser_Fails()
        {
            // Arrange
            var userRequest = new UserRequest { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<CreateUserStartedEvent, UserRequest, CreateUserResponse>(
                    It.IsAny<UserRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateUserResponse)null);

            // Act
            var result = await _controller.CreateUser(userRequest, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetUsers_Successful()
        {
            // Arrange
            var response = new GetUsersResponse { IsSuccess = true, Dto = new List<UserDto> { new UserDto() } };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetUsersStartedEvent, object, GetUsersResponse>(
                    It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUsers(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response.Dto, okResult.Value);
        }

        [Fact]
        public async Task GetUsers_Fails()
        {
            // Arrange
            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetUsersStartedEvent, object, GetUsersResponse>(
                    It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetUsersResponse)null);

            // Act
            var result = await _controller.GetUsers(CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetUserById_Successful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var response = new GetUserResponse { IsSuccess = true, Dto = new UserDto { Id = userId } };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetUserStartedEvent, UserRequest, GetUserResponse>(
                    It.IsAny<UserRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserById(userId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response.Dto, okResult.Value);
        }

        [Fact]
        public async Task GetUserById_Fails()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetUserStartedEvent, UserRequest, GetUserResponse>(
                    It.IsAny<UserRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetUserResponse)null);

            // Act
            var result = await _controller.GetUserById(userId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_Successful()
        {
            // Arrange
            var userRequest = new UserRequest { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };
            var userBaseDto = new UserBaseDto { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };
            var response = new UpdateUserResponse { IsSuccess = true, Dto = userBaseDto };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<UpdateUserStartedEvent, UserRequest, UpdateUserResponse>(
                    It.IsAny<UserRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateUser(userRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response.Dto, okResult.Value);
        }

        [Fact]
        public async Task UpdateUser_Fails()
        {
            // Arrange
            var userRequest = new UserRequest { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<UpdateUserStartedEvent, UserRequest, UpdateUserResponse>(
                    It.IsAny<UserRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateUserResponse)null);

            // Act
            var result = await _controller.UpdateUser(userRequest, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_Successful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var response = new DeleteUserResponse { IsSuccess = true };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<DeleteUserStartedEvent, UserRequest, DeleteUserResponse>(
                    It.IsAny<UserRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteUser(userId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_Fails()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<DeleteUserStartedEvent, UserRequest, DeleteUserResponse>(
                    It.IsAny<UserRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeleteUserResponse)null);

            // Act
            var result = await _controller.DeleteUser(userId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}
