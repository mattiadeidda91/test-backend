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
    public class UpdateAddressHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IAddressService> _addressServiceMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly Mock<ILogger<UpdateAddressStartedHandler>> _loggerMock;
        private readonly IMapper _mapper;
        private readonly UpdateAddressStartedHandler _handler;

        public UpdateAddressHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _addressServiceMock = new Mock<IAddressService>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _loggerMock = new Mock<ILogger<UpdateAddressStartedHandler>>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AddressRequest, Address>().ReverseMap();
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

            _handler = new UpdateAddressStartedHandler(
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
            var @event = new UpdateAddressStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                CorrelationId = "correlation-id-123",
                Activity = new AddressRequest 
                { 
                    Id = Guid.NewGuid(), 
                    Street = "Via Susa", 
                    City = "Milano", 
                    PostalCode = "12345", 
                    Country = "Italia" 
                }
            };

            var addressDb = new Address
            {
                Id = @event.Activity.Id,
                Street = "Via Torino",
                City = "Torino",
                PostalCode = "67890",
                Country = "America"
            };

            _addressServiceMock.Setup(service => service.GetByIdAsync(@event.Activity.Id)).ReturnsAsync(addressDb);
            _addressServiceMock.Setup(service => service.UpdateAsync(It.IsAny<Address>())).Returns(Task.CompletedTask);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _msgBusMock.Verify(bus => bus.SendMessage(
                It.Is<UpdateAddressResponse>(response => response.IsSuccess && response.Dto != null),
                _kafkaOptionsMock.Object.Value.Producers.ConsumerTopic,
                It.IsAny<CancellationToken>(),
                @event.CorrelationId,
                null), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ShouldSendFailureMessage_WhenAddressNotFound()
        {
            // Arrange
            var @event = new UpdateAddressStartedEvent
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
                It.Is<UpdateAddressResponse>(response => !response.IsSuccess && response.Dto == null),
                _kafkaOptionsMock.Object.Value.Producers.ConsumerTopic,
                It.IsAny<CancellationToken>(),
                @event.CorrelationId,
                null), Times.Once);
        }

    }
}
