using AutoMapper;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.Product.Request;
using Test.Backend.Abstractions.Models.Entities;

namespace Test.Backend.Services.ProductService.Mapper
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            /* CATEGORY */
            _ = CreateMap<Abstractions.Models.Dto.Category.Request.CategoryRequest, Category>().ReverseMap();

            _ = CreateMap<CategoryBaseDto, Category>().ReverseMap();
            _ = CreateMap<CategoryDto, Category>().ReverseMap();

            /* PRODUCTS */
            _ = CreateMap<ProductRequest, Product>().ReverseMap();

            _ = CreateMap<ProductBaseDto, Product>().ReverseMap();
            _ = CreateMap<ProductDto, Product>().ReverseMap();
            _ = CreateMap<ProductWithoutCategoryDto, Product>().ReverseMap();
            _ = CreateMap<ProductWithoutOrderDto, Product>().ReverseMap();
        }
    }
}
