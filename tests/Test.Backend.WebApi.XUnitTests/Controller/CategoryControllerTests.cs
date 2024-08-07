using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Dto.Category.Request;
using Test.Backend.Abstractions.Models.Dto.Category.Response;
using Test.Backend.Abstractions.Models.Events.Category;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.WebApi.Controllers.v1;

namespace Test.Backend.WebApi.XUnitTests.Controller
{
    public class CategoryControllerTests
    {
        private readonly Mock<IEventBusService> _mockEventBusService;
        private readonly Mock<IOptions<KafkaOptions>> _mockKafkaOptions;
        private readonly Mock<ILogger<CategoryController>> _mockLogger;
        private readonly KafkaOptions _kafkaOptions;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _mockEventBusService = new Mock<IEventBusService>();
            _mockKafkaOptions = new Mock<IOptions<KafkaOptions>>();
            _mockLogger = new Mock<ILogger<CategoryController>>();

            _kafkaOptions = new KafkaOptions
            {
                Consumers = new Consumers
                {
                    AddressTopic = "product-topic"
                },
                Producers = new Producers
                {
                    ConsumerTopic = "consumer-topic"
                }
            };

            _mockKafkaOptions.Setup(opt => opt.Value).Returns(_kafkaOptions);

            _controller = new CategoryController(_mockEventBusService.Object, _mockKafkaOptions.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateCategory_Successful()
        {
            // Arrange
            var categoryBaseDto = new CategoryBaseDto { Name = "Print" };
            var categoryRequest = new CategoryRequest { Name = "Print" };
            var response = new CreateCategoryResponse { IsSuccess = true, Dto = categoryBaseDto };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<CreateCategoryStartedEvent, CategoryRequest, CreateCategoryResponse>(
                    It.IsAny<CategoryRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateAddress(categoryRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response.Dto, okResult.Value);
        }

        [Fact]
        public async Task CreateCategory_Fails()
        {
            // Arrange
            var categoryRequest = new CategoryRequest { Name = "Electronics" };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<CreateCategoryStartedEvent, CategoryRequest, CreateCategoryResponse>(
                    It.IsAny<CategoryRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateCategoryResponse)null);

            // Act
            var result = await _controller.CreateAddress(categoryRequest, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetCategories_Successful()
        {
            // Arrange
            var response = new GetCategoriesResponse { IsSuccess = true, Dto = new List<CategoryDto> { new CategoryDto() } };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetCategoriesStartedEvent, object, GetCategoriesResponse>(
                    It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetCategories(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response.Dto, okResult.Value);
        }

        [Fact]
        public async Task GetCategories_Fails()
        {
            // Arrange
            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetCategoriesStartedEvent, object, GetCategoriesResponse>(
                    It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetCategoriesResponse)null);

            // Act
            var result = await _controller.GetCategories(CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetCategoryById_ShouldReturnOk_Successful()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var response = new GetCategoryResponse { IsSuccess = true, Dto = new CategoryDto { Id = categoryId } };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetCategoryStartedEvent, CategoryRequest, GetCategoryResponse>(
                    It.IsAny<CategoryRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetCategoryById(categoryId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response.Dto, okResult.Value);
        }

        [Fact]
        public async Task GetCategoryById_Fails()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<GetCategoryStartedEvent, CategoryRequest, GetCategoryResponse>(
                    It.IsAny<CategoryRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetCategoryResponse)null);

            // Act
            var result = await _controller.GetCategoryById(categoryId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task UpdateCategory_Successful()
        {
            // Arrange
            var categoryRequest = new CategoryRequest { Name = "Category" };
            var categoryDto = new CategoryBaseDto { Name = "Category" };
            var response = new UpdateCategoryResponse { IsSuccess = true, Dto = categoryDto };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<UpdateCategoryStartedEvent, CategoryRequest, UpdateCategoryResponse>(
                    It.IsAny<CategoryRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateUser(categoryRequest, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response.Dto, okResult.Value);
        }

        [Fact]
        public async Task UpdateCategory_Fails()
        {
            // Arrange
            var categoryRequest = new CategoryRequest { Name = "Category" };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<UpdateCategoryStartedEvent, CategoryRequest, UpdateCategoryResponse>(
                    It.IsAny<CategoryRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateCategoryResponse)null);

            // Act
            var result = await _controller.UpdateUser(categoryRequest, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_Successful()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var response = new DeleteCategoryResponse { IsSuccess = true };

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<DeleteCategoryStartedEvent, CategoryRequest, DeleteCategoryResponse>(
                    It.IsAny<CategoryRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteUser(categoryId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }
        [Fact]
        public async Task DeleteCategory_Fails()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _mockEventBusService
                .Setup(m => m.HandleMsgBusMessages<DeleteCategoryStartedEvent, CategoryRequest, DeleteCategoryResponse>(
                    It.IsAny<CategoryRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeleteCategoryResponse)null);

            // Act
            var result = await _controller.DeleteUser(categoryId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}
