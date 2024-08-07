using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Services.OrderService.Interfaces;

namespace Test.Backend.Services.OrderService.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> logger;
        private readonly IMapper mapper;
        private readonly IOrderService orderService;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger, IMapper mapper)
        {
            this.orderService = orderService;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
        {
            var users = await orderService.GetAsync();

            var usersDto = mapper.Map<List<OrderDto>>(users);

            return StatusCode(usersDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, usersDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById([Required] Guid id, CancellationToken cancellationToken)
        {
            var orders = await orderService.GetByIdAsync(id);

            var ordersDto = mapper.Map<OrderDto>(orders);

            return StatusCode(ordersDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, ordersDto);
        }
    }
}
