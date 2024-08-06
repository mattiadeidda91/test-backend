using Test.Backend.Abstractions.Models.Dto.Common;
using Test.Backend.Abstractions.Models.Dto.Product;

namespace Test.Backend.Abstractions.Models.Dto.Category
{
    public interface IProduct
    {
        public ICollection<ProductWithoutCategoryDto>? Products { get; set; }
    }
    public class CategoryBaseDto : BaseDto
    {
        public string? Name { get; set; }
    }

    public class CategoryDto : CategoryBaseDto, IProduct
    {
        public ICollection<ProductWithoutCategoryDto>? Products { get; set; }
    }
}
