using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.Product.Request;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.ProductService.Interfaces;

namespace Test.Backend.WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> logger;
        private readonly IMapper mapper;
        private readonly IProductService productService;

        public ProductController(IProductService productService, ILogger<ProductController> logger, IMapper mapper)
        {
            this.productService = productService;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([Required] ProductRequest request)
        {
            var product = mapper.Map<Product>(request);

            if (product != null)
            {
                await productService.SaveAsync(product);

                return Ok();
            }
            else
                return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await productService.GetAsync();

            var productsDtp = mapper.Map<List<ProductDto>>(products);

            return StatusCode(productsDtp != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, productsDtp);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById([Required] Guid id)
        {
            var products = await productService.GetByIdAsync(id);

            var productDtp = mapper.Map<ProductDto>(products);

            return StatusCode(productDtp != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, productDtp);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([Required] ProductRequest request)
        {
            var productDb = await productService.GetByIdAsync(request.Id);

            if (productDb != null)
            {
                var product = mapper.Map<Product>(request);

                if (product != null)
                {
                    await productService.UpdateAsync(product);

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
            var isDeleted = await productService.DeleteAsync(id);

            return StatusCode(isDeleted ? StatusCodes.Status200OK : StatusCodes.Status404NotFound);
        }
    }
}
