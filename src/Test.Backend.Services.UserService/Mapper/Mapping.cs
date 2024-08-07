using AutoMapper;
using Test.Backend.Abstractions.Models.Dto.User.Request;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Entities;

namespace Test.Backend.Services.UserService.Mapper
{
    public class Mapping : Profile
    {
        public Mapping() 
        {
            _ = CreateMap<UserRequest, User>().ReverseMap();

            _ = CreateMap<UserBaseDto, User>().ReverseMap();
            _ = CreateMap<UserDto, User>().ReverseMap();
        }
    }
}
