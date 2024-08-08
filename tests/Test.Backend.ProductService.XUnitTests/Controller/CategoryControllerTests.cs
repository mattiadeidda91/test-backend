using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.ProductService.Controllers.v1;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.ProductService.XUnitTests.Controller
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<CategoryController>> _loggerMock;
        private readonly CategoryController _categoryController;

        public CategoryControllerTests()
        {
            _categoryServiceMock = new Mock<ICategoryService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<CategoryController>>();
            _categoryController = new CategoryController(_categoryServiceMock.Object, _loggerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetCategoriess_Successful()
        {
            // Arrange
            var categories = new List<Category> { new Category { Id = Guid.NewGuid(), Name = "Category1" } };
            var categoriesDto = new List<CategoryDto> { new CategoryDto { Id = Guid.NewGuid(), Name = "Category1" } };

            _categoryServiceMock.Setup(service => service.GetAsync()).ReturnsAsync(categories);
            _mapperMock.Setup(mapper => mapper.Map<List<CategoryDto>>(categories)).Returns(categoriesDto);

            // Act
            var result = await _categoryController.GetCategoriess(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(categoriesDto, okResult.Value);
        }

        [Fact]
        public async Task GetCategoriess_Fails()
        {
            // Arrange
            _categoryServiceMock.Setup(service => service.GetAsync()).ReturnsAsync((List<Category>)null);

            // Act
            var result = await _categoryController.GetCategoriess(CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetCategoryById_Successful()
        {
            // Arrange
            var category = new Category { Id = Guid.NewGuid(), Name = "Category1" };
            var categoryDto = new CategoryDto { Id = Guid.NewGuid(), Name = "Category1" };

            _categoryServiceMock.Setup(service => service.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(category);
            _mapperMock.Setup(mapper => mapper.Map<CategoryDto>(category)).Returns(categoryDto);

            // Act
            var result = await _categoryController.GetCategoryById(category.Id, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(categoryDto, okResult.Value);
        }

        [Fact]
        public async Task GetCategoryById_Fails()
        {
            // Arrange
            _categoryServiceMock.Setup(service => service.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category)null);

            // Act
            var result = await _categoryController.GetCategoryById(Guid.NewGuid(), CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}
