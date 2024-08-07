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
using Test.Backend.Services.AddressService.Handlers;
using Test.Backend.Services.AddressService.Interfaces;

namespace Test.Backend.AddressService.XUnitTests.Handlers
{
    public class DeleteAddressHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IAddressService> _addressServiceMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly Mock<ILogger<DeleteAddressStartedHandler>> _loggerMock;
        private readonly IMapper _mapper;
        private readonly DeleteAddressStartedHandler _handler;

        public DeleteAddressHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _addressServiceMock = new Mock<IAddressService>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _loggerMock = new Mock<ILogger<DeleteAddressStartedHandler>>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Address, AddressBaseDto>().ReverseMap();
            });

            _mapper = configuration.CreateMapper();
            _kafkaOptionsMock.Setup(k => k.Value).Returns(new KafkaOptions
            {
                Producers = new Producers
                {
                    ConsumerTopic = "response-topic"
                }
            });

            _handler = new DeleteAddressStartedHandler(
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
            var @event = new DeleteAddressStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new AddressRequest { Id = Guid.NewGuid() },
                CorrelationId = "correlation-id-123"
            };

            var address = new Address
            {
                Id = @event.Activity.Id,
                Street = "Via Susa",
                City = "Torino",
                PostalCode = "12345",
                Country = "Italia"
            };

            _addressServiceMock.Setup(service => service.GetByIdAsync(@event.Activity.Id)).ReturnsAsync(address);
            _addressServiceMock.Setup(service => service.DeleteAsync(@event.Activity.Id)).ReturnsAsync(true);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _addressServiceMock.Verify(service => service.DeleteAsync(@event.Activity.Id), Times.Once);

            _msgBusMock.Verify(bus => bus.SendMessage(
                It.Is<DeleteAddressResponse>(response => response.IsSuccess && response.Dto.Street == address.Street),
                _kafkaOptionsMock.Object.Value.Producers.ConsumerTopic,
                It.IsAny<CancellationToken>(),
                @event.CorrelationId,
                null), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Fails()
        {
            // Arrange
            var @event = new DeleteAddressStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new AddressRequest { Id = Guid.NewGuid() },
                CorrelationId = "correlation-id-123"
            };

            var address = new Address
            {
                Id = @event.Activity.Id,
                Street = "Via sisa",
                City = "Torino",
                PostalCode = "12345",
                Country = "Italia"
            };

            _addressServiceMock.Setup(service => service.GetByIdAsync(@event.Activity.Id)).ReturnsAsync(address);
            _addressServiceMock.Setup(service => service.DeleteAsync(@event.Activity.Id)).ReturnsAsync(false);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _msgBusMock.Verify(bus => bus.SendMessage(
                It.Is<DeleteAddressResponse>(response => !response.IsSuccess && response.Dto == null),
                _kafkaOptionsMock.Object.Value.Producers.ConsumerTopic,
                It.IsAny<CancellationToken>(),
                @event.CorrelationId,
                null), Times.Once);
        }

    }
}
