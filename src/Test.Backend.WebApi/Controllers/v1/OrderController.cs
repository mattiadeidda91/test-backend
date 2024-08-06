using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Order;
using Test.Backend.Abstractions.Models.Dto.Order.Request;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.OrderService.Interfaces;

namespace Test.Backend.WebApi.Controllers.v1
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

        [HttpPost]
        public async Task<IActionResult> CreateOrder([Required] OrderRequest request)
        {
            var order = mapper.Map<Order>(request);

            if (order != null)
            {
                await orderService.SaveAsync(order);

                return Ok();
            }
            else
                return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await orderService.GetAsync();

            var ordersDto = mapper.Map<List<OrderDto>>(orders);

            return StatusCode(ordersDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, ordersDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById([Required] Guid id)
        {
            var orders = await orderService.GetByIdAsync(id);

            var orderDto = mapper.Map<OrderDto>(orders);

            return StatusCode(orderDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, orderDto);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([Required] OrderRequest request)
        {
            var orderDb = await orderService.GetByIdAsync(request.Id);

            if (orderDb != null)
            {
                var order = mapper.Map<Order>(request);

                if (order != null)
                {
                    await orderService.UpdateAsync(order);

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
            var isDeleted = await orderService.DeleteAsync(id);

            return StatusCode(isDeleted ? StatusCodes.Status200OK : StatusCodes.Status404NotFound);
        }
    }
}
