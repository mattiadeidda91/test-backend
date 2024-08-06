using AutoMapper;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Address.Request;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Dto.Category.Request;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Request;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.Product.Request;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Abstractions.Models.Dto.User.Request;
using Test.Backend.Abstractions.Models.Entities;

namespace Test.Backend.WebApi.Mapper
{
    public class Mapping : Profile
    {
        public Mapping() 
        {
            /* USERS */
            _ = CreateMap<UserRequest, User>().ReverseMap();

            _ = CreateMap<UserBaseDto, User>().ReverseMap();
            _ = CreateMap<UserDto, User>().ReverseMap();

            /* ADDRESS */
            _ = CreateMap<AddressRequest, Address>().ReverseMap();

            _ = CreateMap<Address, AddressBaseDto>().ReverseMap();
            _ = CreateMap<Address, AddressDto>().ReverseMap();

            /* CATEGORY */
            _ = CreateMap<CategoryRequest, Category>().ReverseMap();

            _ = CreateMap<CategoryBaseDto, Category>().ReverseMap();
            _ = CreateMap<CategoryDto, Category>().ReverseMap();

            /* PRODUCTS */
            _ = CreateMap<ProductRequest, Product>().ReverseMap();

            _ = CreateMap<ProductBaseDto, Product>().ReverseMap();
            _ = CreateMap<ProductDto, Product>().ReverseMap();
            _ = CreateMap<ProductWithoutCategoryDto, Product>().ReverseMap();
            _ = CreateMap<ProductWithoutOrderDto, Product>().ReverseMap();

            /* ORDERS */
            _ = CreateMap<OrderRequest, Order>()
                .ForMember(dest => dest.OrderProducts, opt => opt.MapFrom(src =>
                    src.ProductIds != null
                    ? src.ProductIds.Select(id => new OrderProduct { Id = Guid.NewGuid(), ProductId = id }).ToList()
                    : new List<OrderProduct>()));

            _ = CreateMap<OrderBaseDto, Order>().ReverseMap();
            
            _ = CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom( src => src.DeliveryAddress))
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src =>
                    src.OrderProducts.Select(p => p.Product).ToList()))
                .ReverseMap();
            
            _ = CreateMap<Order, OrderWithoutAddressDto>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src =>
                    src.OrderProducts.Select(p => p.Product).ToList()))
                .ReverseMap();
            
            _ = CreateMap<Order, OrderWithoutUserDto>()
                .ReverseMap();

            _ = CreateMap<Order, OrderWithoutProductDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.DeliveryAddress))
                .ReverseMap();
        }
    }
}
