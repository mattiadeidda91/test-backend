using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Address.Request;
using Test.Backend.Abstractions.Models.Dto.Address.Response;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Events.Address;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Services.AddressService.Interfaces;
using Test.Backend.Services.UserService.Handlers;

namespace Test.Backend.AddressService.XUnitTests.Handlers
{
    public class GetAddressHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IAddressService> _addressServiceMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly Mock<ILogger<GetAddressStartedHandler>> _loggerMock;
        private readonly IMapper _mapper;
        private readonly GetAddressStartedHandler _handler;

        public GetAddressHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _addressServiceMock = new Mock<IAddressService>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _loggerMock = new Mock<ILogger<GetAddressStartedHandler>>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Address, AddressDto>().ReverseMap();
            });

            _mapper = configuration.CreateMapper();
            _kafkaOptionsMock.Setup(k => k.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });

            _handler = new GetAddressStartedHandler(
                _msgBusMock.Object,
                _addressServiceMock.Object,
                _mapper,
                _kafkaOptionsMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task HandleAsync_Successful()
        {
            // Arrange
            var @event = new GetAddressStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                CorrelationId = "correlation-id-123",
                Activity = new AddressRequest { Id = Guid.NewGuid() }
            };

            var address = new Address
            {
                Street = "Via Turati",
                City = "Milano",
                PostalCode = "12345",
                Country = "Italia"
            };

            _addressServiceMock.Setup(service => service.GetByIdAsync(@event.Activity.Id)).ReturnsAsync(address);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _msgBusMock.Verify(bus => bus.SendMessage(
                It.Is<GetAddressResponse>(response => response.IsSuccess && response.Dto != null),
                _kafkaOptionsMock.Object.Value.Producers.ConsumerTopic,
                It.IsAny<CancellationToken>(),
                @event.CorrelationId,
                null), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var @event = new GetAddressStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                CorrelationId = "correlation-id-123",
                Activity = new AddressRequest { Id = Guid.NewGuid() }
            };

            _addressServiceMock.Setup(service => service.GetByIdAsync(@event.Activity.Id)).ReturnsAsync((Address?)null);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _msgBusMock.Verify(bus => bus.SendMessage(
                It.Is<GetAddressResponse>(response => !response.IsSuccess && response.Dto == null),
                _kafkaOptionsMock.Object.Value.Producers.ConsumerTopic,
                It.IsAny<CancellationToken>(),
                @event.CorrelationId,
                null), Times.Once);
        }


    }
}
