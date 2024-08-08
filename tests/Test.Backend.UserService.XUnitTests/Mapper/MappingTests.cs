using AutoMapper;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Dto.User.Request;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.UserService.Mapper;

namespace Test.Backend.UserService.XUnitTests.Mapper
{
    public class MappingTests
    {
        private readonly IMapper _mapper;

        public MappingTests()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Mapping>();
            });

            _mapper = configuration.CreateMapper();
        }

        [Fact]
        public void UserRequest_To_User()
        {
            // Arrange
            var userRequest = new UserRequest
            {
                Id = Guid.NewGuid(),
                FirstName = "Matt",
                LastName = "Test",
                Email = "matt.test@example.com"
            };

            // Act
            var user = _mapper.Map<User>(userRequest);

            // Assert
            Assert.Equal(userRequest.Id, user.Id);
            Assert.Equal(userRequest.FirstName, user.FirstName);
            Assert.Equal(userRequest.LastName, user.LastName);
            Assert.Equal(userRequest.Email, user.Email);
        }

        [Fact]
        public void User_To_UserRequest()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Matt",
                LastName = "Test",
                Email = "matt.test@example.com"
            };

            // Act
            var userRequest = _mapper.Map<UserRequest>(user);

            // Assert
            Assert.Equal(user.Id, userRequest.Id);
            Assert.Equal(user.FirstName, userRequest.FirstName);
            Assert.Equal(user.LastName, userRequest.LastName);
            Assert.Equal(user.Email, userRequest.Email);
        }

        [Fact]
        public void UserBaseDto_To_User()
        {
            // Arrange
            var userBaseDto = new UserBaseDto
            {
                Id = Guid.NewGuid(),
                FirstName = "Matt",
                LastName = "Test",
                Email = "matt.test@example.com"
            };

            // Act
            var user = _mapper.Map<User>(userBaseDto);

            // Assert
            Assert.Equal(userBaseDto.Id, user.Id);
            Assert.Equal(userBaseDto.FirstName, user.FirstName);
            Assert.Equal(userBaseDto.LastName, user.LastName);
            Assert.Equal(userBaseDto.Email, user.Email);
        }

        [Fact]
        public void User_To_UserBaseDto()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Matt",
                LastName = "Test",
                Email = "matt.test@example.com"
            };

            // Act
            var userBaseDto = _mapper.Map<UserBaseDto>(user);

            // Assert
            Assert.Equal(user.Id, userBaseDto.Id);
            Assert.Equal(user.FirstName, userBaseDto.FirstName);
            Assert.Equal(user.LastName, userBaseDto.LastName);
            Assert.Equal(user.Email, userBaseDto.Email);
        }

        [Fact]
        public void UserDto_To_User()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice.johnson@example.com"
            };

            // Act
            var user = _mapper.Map<User>(userDto);

            // Assert
            Assert.Equal(userDto.Id, user.Id);
            Assert.Equal(userDto.FirstName, user.FirstName);
            Assert.Equal(userDto.LastName, user.LastName);
            Assert.Equal(userDto.Email, user.Email);
        }

        [Fact]
        public void Should_Map_User_To_UserDto()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Robert",
                LastName = "Brown",
                Email = "robert.brown@example.com"
            };

            // Act
            var userDto = _mapper.Map<UserDto>(user);

            // Assert
            Assert.Equal(user.Id, userDto.Id);
            Assert.Equal(user.FirstName, userDto.FirstName);
            Assert.Equal(user.LastName, userDto.LastName);
            Assert.Equal(user.Email, userDto.Email);
        }
    }
}
