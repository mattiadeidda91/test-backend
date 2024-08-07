using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.AddressService.Interfaces;
using Test.Backend.Services.AddressService.Mapper;
using Test.Backend.Services.UserService.Controllers.v1;

namespace Test.Backend.AddressService.XUnitTests.Controller
{
    public class AddressControllerTests
    {
        private readonly Mock<IAddressService> _addressServiceMock;
        private readonly Mock<ILogger<AddressController>> _loggerMock;
        private readonly IMapper _mapper;
        private readonly AddressController _controller;

        public AddressControllerTests()
        {
            _addressServiceMock = new Mock<IAddressService>();
            _loggerMock = new Mock<ILogger<AddressController>>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Mapping>();
            });

            _mapper = configuration.CreateMapper();
            _controller = new AddressController(_addressServiceMock.Object, _loggerMock.Object, _mapper);
        }

        [Fact]
        public async Task GetAddress_Successful()
        {
            // Arrange
            var addresses = new List<Address>
            {
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "Via torino",
                    City = "Torino",
                    PostalCode = "12345",
                    Country = "Italy"
                }
            };

            _addressServiceMock.Setup(service => service.GetAsync()).ReturnsAsync(addresses);

            // Act
            var result = await _controller.GetAddress(CancellationToken.None) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var addressesDto = Assert.IsType<List<AddressDto>>(result.Value);

            Assert.Single(addressesDto);
            Assert.Equal(addresses.First().Street, addressesDto.First().Street);
        }

        [Fact]
        public async Task GetAddressById_Successful()
        {
            // Arrange
            var addressId = Guid.NewGuid();
            var address = new Address
            {
                Id = Guid.NewGuid(),
                Street = "Via torino",
                City = "Torino",
                PostalCode = "12345",
                Country = "Italy"
            };

            _addressServiceMock.Setup(service => service.GetByIdAsync(addressId)).ReturnsAsync(address);

            // Act
            var result = await _controller.GetAddressById(addressId, CancellationToken.None) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var addressDto = Assert.IsType<AddressDto>(result.Value);
            Assert.Equal(address.Street, addressDto.Street);
            Assert.Equal(address.City, addressDto.City);
            Assert.Equal(address.PostalCode, addressDto.PostalCode);
            Assert.Equal(address.Country, addressDto.Country);
        }

        [Fact]
        public async Task GetAddressById_Fails()
        {
            // Arrange
            var addressId = Guid.NewGuid();
            _addressServiceMock.Setup(service => service.GetByIdAsync(addressId)).ReturnsAsync((Address)null);

            // Act
            var result = await _controller.GetAddressById(addressId, CancellationToken.None) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
            Assert.Null(result.Value);
        }


    }
}
