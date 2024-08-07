using AutoMapper;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Address.Request;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.AddressService.Mapper;

namespace Test.Backend.AddressService.XUnitTests.Mapper
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
        public void AddressRequest_To_Address()
        {
            // Arrange
            var addressRequest = new AddressRequest
            {
                Id = Guid.NewGuid(),
                Street = "Via Susa",
                City = "Turin",
                PostalCode = "12345",
                Country = "Italy"
            };

            // Act
            var address = _mapper.Map<Address>(addressRequest);

            // Assert
            Assert.Equal(addressRequest.Street, address.Street);
            Assert.Equal(addressRequest.City, address.City);
            Assert.Equal(addressRequest.PostalCode, address.PostalCode);
            Assert.Equal(addressRequest.Country, address.Country);
            Assert.Equal(addressRequest.Id, address.Id);
        }

        [Fact]
        public void Address_To_AddressBaseDto()
        {
            // Arrange
            var address = new Address
            {
                Id = Guid.NewGuid(),
                Street = "Via Susa",
                City = "Turin",
                PostalCode = "12345",
                Country = "Italy"
            };

            // Act
            var addressBaseDto = _mapper.Map<AddressBaseDto>(address);

            // Assert
            Assert.Equal(address.Street, addressBaseDto.Street);
            Assert.Equal(address.City, addressBaseDto.City);
            Assert.Equal(address.PostalCode, addressBaseDto.PostalCode);
            Assert.Equal(address.Country, addressBaseDto.Country);
            Assert.Equal(address.Id, addressBaseDto.Id);
        }

        [Fact]
        public void AddressBaseDto_To_Address()
        {
            // Arrange
            var addressBaseDto = new AddressBaseDto
            {
                Id = Guid.NewGuid(),
                Street = "Via Susa",
                City = "Turin",
                PostalCode = "12345",
                Country = "Italy"
            };

            // Act
            var address = _mapper.Map<Address>(addressBaseDto);

            // Assert
            Assert.Equal(addressBaseDto.Street, address.Street);
            Assert.Equal(addressBaseDto.City, address.City);
            Assert.Equal(addressBaseDto.PostalCode, address.PostalCode);
            Assert.Equal(addressBaseDto.Country, address.Country);
            Assert.Equal(addressBaseDto.Id, address.Id);
        }

        [Fact]
        public void Address_To_AddressDto()
        {
            // Arrange
            var address = new Address
            {
                Id = Guid.NewGuid(),
                Street = "Via Susa",
                City = "Turin",
                PostalCode = "12345",
                Country = "Italy"
            };

            // Act
            var addressDto = _mapper.Map<AddressDto>(address);

            // Assert
            Assert.Equal(address.Street, addressDto.Street);
            Assert.Equal(address.City, addressDto.City);
            Assert.Equal(address.PostalCode, addressDto.PostalCode);
            Assert.Equal(address.Country, addressDto.Country);
            Assert.Equal(address.Id, addressDto.Id);
            Assert.Null(addressDto.Orders);
        }

        [Fact]
        public void AddressDto_To_Address()
        {
            // Arrange
            var addressDto = new AddressDto
            {
                Id = Guid.NewGuid(),
                Street = "Via Susa",
                City = "Turin",
                PostalCode = "12345",
                Country = "Italy",
                Orders = new List<OrderWithoutAddressDto>() 
                {
                    new()
                    { 
                        Id = Guid.NewGuid(),
                        User = new()
                        {
                            Id = Guid.NewGuid(),
                            Email = "test@test.it",
                            FirstName = "Matt",
                            LastName = "test"
                        },
                        OrderDate= DateTime.Now,
                        Products = new List<ProductDto>()
                        {
                            new()
                            { 
                                Id = Guid.NewGuid(),
                                Category = new(){Id= Guid.NewGuid(), Name = "Category"},
                                Name = "Photo",
                                Price = 200
                            }
                        }
                    }
                }
            };

            // Act
            var address = _mapper.Map<Address>(addressDto);

            // Assert
            Assert.Equal(addressDto.Street, address.Street);
            Assert.Equal(addressDto.City, address.City);
            Assert.Equal(addressDto.PostalCode, address.PostalCode);
            Assert.Equal(addressDto.Country, address.Country);
            Assert.Equal(addressDto.Id, address.Id);
        }

    }
}
