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
    public class CreateAddressHandlerTests
    {
        private readonly Mock<IEventBusService> _msgBusMock;
        private readonly Mock<IAddressService> _addressServiceMock;
        private readonly Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
        private readonly Mock<ILogger<CreateAddressStartedHandler>> _loggerMock;
        private readonly IMapper _mapper;
        private readonly CreateAddressStartedHandler _handler;

        public CreateAddressHandlerTests()
        {
            _msgBusMock = new Mock<IEventBusService>();
            _addressServiceMock = new Mock<IAddressService>();
            _kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
            _loggerMock = new Mock<ILogger<CreateAddressStartedHandler>>();

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

            _handler = new CreateAddressStartedHandler(
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
            var @event = new CreateAddressStartedEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = new AddressRequest
                {
                    Street = "Via Susa",
                    City = "Torino",
                    PostalCode = "12345",
                    Country = "Italia"
                },
                CorrelationId = "correlation-id-123"
            };

            var address = _mapper.Map<Address>(@event.Activity);

            _addressServiceMock.Setup(service => service.SaveAsync(It.IsAny<Address>())).Returns(Task.CompletedTask);

            // Act
            await _handler.HandleAsync(@event);

            // Assert
            _addressServiceMock.Verify(service => service.SaveAsync(It.IsAny<Address>()), Times.Once);

            _msgBusMock.Verify(bus => bus.SendMessage(
                It.Is<CreateAddressResponse>(response => response.IsSuccess && response.Dto.Street == address.Street),
                _kafkaOptionsMock.Object.Value.Producers.ConsumerTopic,
                It.IsAny<CancellationToken>(),
                @event.CorrelationId,
                null), Times.Once);
        }

    }
}
