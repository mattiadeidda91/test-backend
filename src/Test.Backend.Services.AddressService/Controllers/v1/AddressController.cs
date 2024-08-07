using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Services.AddressService.Interfaces;

namespace Test.Backend.Services.UserService.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly ILogger<AddressController> logger;
        private readonly IMapper mapper;
        private readonly IAddressService addressService;

        public AddressController(IAddressService addressService, ILogger<AddressController> logger, IMapper mapper)
        {
            this.addressService = addressService;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAddress(CancellationToken cancellationToken)
        {
            var addresses = await addressService.GetAsync();

            var addressesDto = mapper.Map<List<AddressDto>>(addresses);

            return StatusCode(addressesDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, addressesDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressById([Required] Guid id, CancellationToken cancellationToken)
        {
            var address = await addressService.GetByIdAsync(id);

            var addressDto= mapper.Map<AddressDto>(address);

            return StatusCode(addressDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, addressDto);
        }
    }
}