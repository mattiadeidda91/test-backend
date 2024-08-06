using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Category;
using Test.Backend.Abstractions.Models.Dto.Category.Request;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.ProductService.Interfaces;

namespace Test.Backend.WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ILogger<CategoryController> logger;
        private readonly IMapper mapper;
        private readonly ICategoryService categoryService;

        public CategoryController(ICategoryService addressService, ILogger<CategoryController> logger, IMapper mapper)
        {
            this.categoryService = addressService;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress([Required] CategoryRequest request)
        {
            var category = mapper.Map<Category>(request);

            if (category != null)
            {
                await categoryService.SaveAsync(category);

                return Ok();
            }
            else
                return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await categoryService.GetAsync();

            var categoriesDto = mapper.Map<List<CategoryDto>>(categories);

            return StatusCode(categoriesDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, categoriesDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById([Required] Guid id)
        {
            var categories = await categoryService.GetByIdAsync(id);

            var categoryDto = mapper.Map<CategoryDto>(categories);

            return StatusCode(categoryDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, categoryDto);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([Required] CategoryRequest request)
        {
            var categoryDb = await categoryService.GetByIdAsync(request.Id);

            if (categoryDb != null)
            {
                var category = mapper.Map<Category>(request);

                if (category != null)
                {
                    await categoryService.UpdateAsync(category);

                    return Ok();
                }
                else
                    return BadRequest();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([Required] Guid id)
        {
            var isDeleted = await categoryService.DeleteAsync(id);

            return StatusCode(isDeleted ? StatusCodes.Status200OK : StatusCodes.Status404NotFound);
        }
    }
}
