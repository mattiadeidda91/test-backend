using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.UserService.Controllers.v1;
using Test.Backend.Services.UserService.Interfaces;

namespace Test.Backend.UserService.XUnitTests.Controller
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<UserController>>();
            _controller = new UserController(_mockUserService.Object, _mockLogger.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetUsers_Successufl()
        {
            // Arrange
            var userList = new List<User>
            {
                new User { Id = Guid.NewGuid(), FirstName = "Matt", LastName = "Dei", Email = "matt@example.com" },
                new User { Id = Guid.NewGuid(), FirstName = "Carlo", LastName = "Loi", Email = "carlo@example.com" }
            };

            var userDtoList = new List<UserDto>
            {
                new UserDto { Id = Guid.NewGuid(), FirstName = "Matt", LastName = "Dei", Email = "matt@example.com" },
                new UserDto { Id = Guid.NewGuid(), FirstName = "Carlo", LastName = "Loi", Email = "carlo@example.com" }
            };

            _mockUserService.Setup(service => service.GetAsync()).ReturnsAsync(userList);
            _mockMapper.Setup(mapper => mapper.Map<List<UserDto>>(userList)).Returns(userDtoList);

            // Act
            var result = await _controller.GetUsers(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(userDtoList, okResult.Value);
        }

        [Fact]
        public async Task GetUserById_Successful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = Guid.NewGuid(), FirstName = "Matt", LastName = "Dei", Email = "matt@example.com" };
            var userDto = new UserDto { Id = Guid.NewGuid(), FirstName = "Matt", LastName = "Dei", Email = "matt@example.com" };

            _mockUserService.Setup(service => service.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockMapper.Setup(mapper => mapper.Map<UserDto>(user)).Returns(userDto);

            // Act
            var result = await _controller.GetUserById(userId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(userDto, okResult.Value);
        }

        [Fact]
        public async Task GetUserById_Fails()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserService.Setup(service => service.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUserById(userId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }


    }
}
