using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.Services.UserService.Interfaces;

namespace Test.Backend.Services.UserService.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly IMapper mapper;
        private readonly IUserService userService;

        public UserController(IUserService userService, ILogger<UserController> logger, IMapper mapper)
        {
            this.userService = userService;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var users = await userService.GetAsync();

            var usersDto = mapper.Map<List<UserDto>>(users);

            return StatusCode(usersDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, usersDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([Required] Guid id, CancellationToken cancellationToken)
        {
            var users = await userService.GetByIdAsync(id);

            var userDto = mapper.Map<UserDto>(users);

            return StatusCode(userDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, userDto);
        }
    }
}