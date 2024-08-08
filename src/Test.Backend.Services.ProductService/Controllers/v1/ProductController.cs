using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.Services.ProductService.Controllers.v1
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

        [HttpGet]
        public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
        {
            var products = await productService.GetAsync();

            var productsDto = mapper.Map<List<ProductDto>>(products);

            return StatusCode(productsDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, productsDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById([Required] Guid id, CancellationToken cancellationToken)
        {
            var product = await productService.GetByIdAsync(id);

            var productDto = mapper.Map<ProductDto>(product);

            return StatusCode(productDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, productDto);
        }
    }
}