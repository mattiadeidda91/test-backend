using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Address.Request;
using Test.Backend.Abstractions.Models.Dto.Address.Response;
using Test.Backend.Abstractions.Models.Events.Address;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.WebApi.Controllers.v1;

namespace Test.Backend.WebApi.XUnitTests.Controller
{
    public class AddressControllerTests
    {
        private readonly Mock<IEventBusService> _mockEventBusService;
        private readonly Mock<IOptions<KafkaOptions>> _mockKafkaOptions;
        private readonly Mock<ILogger<AddressController>> _mockLogger;
        private readonly KafkaOptions _kafkaOptions;
        private readonly AddressController _controller;

        public AddressControllerTests()
        {
            _mockEventBusService = new Mock<IEventBusService>();
            _mockKafkaOptions = new Mock<IOptions<KafkaOptions>>();
            _mockLogger = new Mock<ILogger<AddressController>>();

            _kafkaOptions = new KafkaOptions
            {
                Consumers = new Consumers
                {
                    AddressTopic = "address-topic"
                },
                Producers = new Producers
                {
                    ConsumerTopic = "consumer-topic"
                }
            };

            _mockKafkaOptions.Setup(opt => opt.Value).Returns(_kafkaOptions);

            _controller = new AddressController(_mockEventBusService.Object, _mockKafkaOptions.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateAddress_Successful()
        {
            // Arrange
            var addressRequest = new AddressRequest();
            var response = new CreateAddressResponse { IsSuccess = true };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<CreateAddressStartedEvent, AddressRequest, CreateAddressResponse>(
                    It.IsAny<AddressRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateAddress(addressRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task CreateAddress_ShouldReturnBadRequest_WhenCreationFails()
        {
            // Arrange
            var addressRequest = new AddressRequest();

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<CreateAddressStartedEvent, AddressRequest, CreateAddressResponse>(
                    It.IsAny<AddressRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateAddressResponse)null);

            // Act
            var result = await _controller.CreateAddress(addressRequest, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetAddressById_ShouldReturnOk_WhenAddressIsFound()
        {
            // Arrange
            var addressId = Guid.NewGuid();
            var response = new GetAddressResponse { IsSuccess = true, Dto = new AddressDto { Id = addressId } };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetAddressStartedEvent, AddressRequest, GetAddressResponse>(
                    It.IsAny<AddressRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAddressById(addressId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetAddressById_Fails()
        {
            // Arrange
            var addressId = Guid.NewGuid();
            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetAddressStartedEvent, AddressRequest, GetAddressResponse>(
                    It.IsAny<AddressRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetAddressResponse)null);

            // Act
            var result = await _controller.GetAddressById(addressId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetAddresses_Fails()
        {
            // Arrange
            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetAddressesStartedEvent, object, GetAddressesResponse>(
                    It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetAddressesResponse)null);

            // Act
            var result = await _controller.GetAddresses(CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetAddresses_Successful()
        {
            // Arrange
            var response = new GetAddressesResponse { IsSuccess = true, Dto = new List<AddressDto> { new AddressDto { Id = Guid.NewGuid() } } };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetAddressesStartedEvent, object, GetAddressesResponse>(
                    It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAddresses(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task UpdateAddress_Successful()
        {
            // Arrange
            var addressRequest = new AddressRequest();
            var response = new UpdateAddressResponse { IsSuccess = true, Dto = new AddressBaseDto { Id = Guid.NewGuid() } };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<UpdateAddressStartedEvent, AddressRequest, UpdateAddressResponse>(
                    It.IsAny<AddressRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateAddress(addressRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task UpdateAddress_Fails()
        {
            // Arrange
            var addressRequest = new AddressRequest();

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<UpdateAddressStartedEvent, AddressRequest, UpdateAddressResponse>(
                    It.IsAny<AddressRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateAddressResponse)null);

            // Act
            var result = await _controller.UpdateAddress(addressRequest, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAddress_Successful()
        {
            // Arrange
            var addressId = Guid.NewGuid();
            var response = new DeleteAddressResponse { IsSuccess = true };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<DeleteAddressStartedEvent, AddressRequest, DeleteAddressResponse>(
                    It.IsAny<AddressRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteAddress(addressId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAddress_Fails()
        {
            // Arrange
            var addressId = Guid.NewGuid();

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<DeleteAddressStartedEvent, AddressRequest, DeleteAddressResponse>(
                    It.IsAny<AddressRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeleteAddressResponse)null);

            // Act
            var result = await _controller.DeleteAddress(addressId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, notFoundResult.StatusCode);
        }
    }
}
