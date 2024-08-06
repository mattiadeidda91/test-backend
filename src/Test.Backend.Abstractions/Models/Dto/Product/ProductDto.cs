using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Dto.Common;
using Test.Backend.Abstractions.Models.Dto.Order;

namespace Test.Backend.Abstractions.Models.Dto.Product
{
    public interface ICategory
    {
        public CategoryBaseDto? Category { get; set; }
    }

    public interface IOrder
    {
        public ICollection<OrderWithoutProductDto>? Orders { get; set; }
    }

    public class ProductBaseDto : BaseDto
    {
        public string? Name { get; set; }
        public double Price { get; set; }
    }

    public class ProductWithoutCategoryDto : BaseDto, IOrder
    {
        public string? Name { get; set; }
        public double Price { get; set; }
        public ICollection<OrderWithoutProductDto>? Orders { get; set; }
    }

    public class ProductWithoutOrderDto : BaseDto, ICategory
    {
        public string? Name { get; set; }
        public double Price { get; set; }
        public CategoryBaseDto? Category { get; set; }
    }

    public class ProductDto : BaseDto, ICategory, IOrder
    {
        public string? Name { get; set; }
        public double Price { get; set; }
        public CategoryBaseDto? Category { get; set; }
        public ICollection<OrderWithoutProductDto>? Orders { get; set; }
    }
}
