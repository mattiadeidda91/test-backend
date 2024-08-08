using AutoMapper;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Dto.Category.Request;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.Product.Request;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.ProductService.Mapper;

namespace Test.Backend.ProductService.XUnitTests.Mapper
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
        public void Map_CategoryRequest_ToCategory()
        {
            // Arrange
            var categoryRequest = new CategoryRequest
            {
                Id = Guid.NewGuid(),
                Name = "Test Category"
            };

            // Act
            var category = _mapper.Map<Category>(categoryRequest);

            // Assert
            Assert.Equal(categoryRequest.Id, category.Id);
            Assert.Equal(categoryRequest.Name, category.Name);
        }

        [Fact]
        public void Map_Category_ToCategoryRequest()
        {
            // Arrange
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Test Category"
            };

            // Act
            var categoryRequest = _mapper.Map<CategoryRequest>(category);

            // Assert
            Assert.Equal(category.Id, categoryRequest.Id);
            Assert.Equal(category.Name, categoryRequest.Name);
        }

        [Fact]
        public void Map_CategoryDto_ToCategory()
        {
            // Arrange
            var categoryDto = new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Products = new List<ProductWithoutCategoryDto>()
            };

            // Act
            var category = _mapper.Map<Category>(categoryDto);

            // Assert
            Assert.Equal(categoryDto.Id, category.Id);
            Assert.Equal(categoryDto.Name, category.Name);
        }

        [Fact]
        public void Map_Category_ToCategoryDto()
        {
            // Arrange
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Products = new List<Product>()
            };

            // Act
            var categoryDto = _mapper.Map<CategoryDto>(category);

            // Assert
            Assert.Equal(category.Id, categoryDto.Id);
            Assert.Equal(category.Name, categoryDto.Name);
        }

        [Fact]
        public void Map_ProductRequest_ToProduct()
        {
            // Arrange
            var productRequest = new ProductRequest
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                Price = 10.99,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var product = _mapper.Map<Product>(productRequest);

            // Assert
            Assert.Equal(productRequest.Id, product.Id);
            Assert.Equal(productRequest.Name, product.Name);
            Assert.Equal(productRequest.Price, product.Price);
            Assert.Equal(productRequest.CategoryId, product.CategoryId);
        }

        [Fact]
        public void Map_Product_ToProductRequest_()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                Price = 10.99,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var productRequest = _mapper.Map<ProductRequest>(product);

            // Assert
            Assert.Equal(product.Id, productRequest.Id);
            Assert.Equal(product.Name, productRequest.Name);
            Assert.Equal(product.Price, productRequest.Price);
            Assert.Equal(product.CategoryId, productRequest.CategoryId);
        }

        [Fact]
        public void Map_ProductDto_ToProduct()
        {
            // Arrange
            var productDto = new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                Price = 10.99,
                Category = new CategoryBaseDto { Id = Guid.NewGuid(), Name = "Test Category" },
                Orders = new List<OrderWithoutProductDto>()
            };

            // Act
            var product = _mapper.Map<Product>(productDto);

            // Assert
            Assert.Equal(productDto.Id, product.Id);
            Assert.Equal(productDto.Name, product.Name);
            Assert.Equal(productDto.Price, product.Price);
        }

        [Fact]
        public void Map_Product_ToProductDto()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                Price = 10.99,
                Category = new Category { Id = Guid.NewGuid(), Name = "Test Category" }
            };

            // Act
            var productDto = _mapper.Map<ProductDto>(product);

            // Assert
            Assert.Equal(product.Id, productDto.Id);
            Assert.Equal(product.Name, productDto.Name);
            Assert.Equal(product.Price, productDto.Price);
        }
    }
}
