using AutoMapper;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Request;
using Test.Backend.Abstractions.Models.Entities;

namespace Test.Backend.Services.OrderService.Mapper
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            /* ORDERS */
            _ = CreateMap<Order, OrderRequest>().ReverseMap()
                .ForMember(dest => dest.OrderProducts, opt => opt.MapFrom(src =>
                    src.ProductIds != null
                    ? src.ProductIds.Select(id => new OrderProduct { Id = Guid.NewGuid(), ProductId = id }).ToList()
                    : new List<OrderProduct>()));

            _ = CreateMap<OrderBaseDto, Order>().ReverseMap();

            _ = CreateMap<Order, OrderDto>()
                //.ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.DeliveryAddress))
                //.ForMember(dest => dest.Products, opt => opt.MapFrom(src =>
                //    src.OrderProducts.Select(p => p.Product).ToList()))
                .ReverseMap();

            _ = CreateMap<Order, OrderWithoutAddressDto>()
                //.ForMember(dest => dest.Products, opt => opt.MapFrom(src =>
                //    src.OrderProducts.Select(p => p.Product).ToList()))
                .ReverseMap();

            _ = CreateMap<Order, OrderWithoutUserDto>()
                .ReverseMap();

            _ = CreateMap<Order, OrderWithoutProductDto>()
                //.ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.DeliveryAddress))
                .ReverseMap();
        }
    }
}
