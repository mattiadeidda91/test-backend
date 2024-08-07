using AutoMapper;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Address.Request;
using Test.Backend.Abstractions.Models.Entities;

namespace Test.Backend.Services.AddressService.Mapper
{
    public class Mapping : Profile
    {
        public Mapping() 
        {
            _ = CreateMap<AddressRequest, Address>().ReverseMap();

            _ = CreateMap<Address, AddressBaseDto>().ReverseMap();
            _ = CreateMap<Address, AddressDto>().ReverseMap();

        }
    }
}
