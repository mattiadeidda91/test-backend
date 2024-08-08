using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.ProductService.Controllers.v1;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.ProductService.XUnitTests.Controller
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<ProductController>> _loggerMock;
        private readonly ProductController _productController;

        public ProductControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<ProductController>>();
            _productController = new ProductController(_productServiceMock.Object, _loggerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetProducts_Successful()
        {
            // Arrange
            var products = new List<Product> { new Product { Id = Guid.NewGuid(), Name = "Product1", Price = 10.0 } };
            var productsDto = new List<ProductDto> { new ProductDto { Id = Guid.NewGuid(), Name = "Product1", Price = 10.0 } };

            _productServiceMock.Setup(service => service.GetAsync()).ReturnsAsync(products);
            _mapperMock.Setup(mapper => mapper.Map<List<ProductDto>>(products)).Returns(productsDto);

            // Act
            var result = await _productController.GetProducts(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(productsDto, okResult.Value);
        }

        [Fact]
        public async Task GetProducts_Fails()
        {
            // Arrange
            _productServiceMock.Setup(service => service.GetAsync()).ReturnsAsync((List<Product>)null);

            // Act
            var result = await _productController.GetProducts(CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetProductById_Successful()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Name = "Product1", Price = 10.0 };
            var productDto = new ProductDto { Id = Guid.NewGuid(), Name = "Product1", Price = 10.0 };

            _productServiceMock.Setup(service => service.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);
            _mapperMock.Setup(mapper => mapper.Map<ProductDto>(product)).Returns(productDto);

            // Act
            var result = await _productController.GetProductById(product.Id, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(productDto, okResult.Value);
        }

        [Fact]
        public async Task GetProductById_Fails()
        {
            // Arrange
            _productServiceMock.Setup(service => service.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product)null);

            // Act
            var result = await _productController.GetProductById(Guid.NewGuid(), CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}
