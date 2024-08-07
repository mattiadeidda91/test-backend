using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.Services.UserService.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ILogger<CategoryController> logger;
        private readonly IMapper mapper;
        private readonly ICategoryService categoryService;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger, IMapper mapper)
        {
            this.categoryService = categoryService;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriess(CancellationToken cancellationToken)
        {
            var categories = await categoryService.GetAsync();

            var categoriesDto = mapper.Map<List<CategoryDto>>(categories);

            return StatusCode(categoriesDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, categoriesDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById([Required] Guid id, CancellationToken cancellationToken)
        {
            var category = await categoryService.GetByIdAsync(id);

            var categoryDto = mapper.Map<CategoryDto>(category);

            return StatusCode(categoryDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, categoryDto);
        }
    }
}