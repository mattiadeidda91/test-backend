using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Address.Request;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.AddressService.Interfaces;

namespace Test.Backend.WebApi.Controllers.v1
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

        [HttpPost]
        public async Task<IActionResult> CreateAddress([Required] AddressRequest request)
        {
            var address = mapper.Map<Address>(request);

            if (address != null)
            {
                await addressService.SaveAsync(address);

                return Ok();
            }
            else
                return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            var addresses = await addressService.GetAsync();

            var addressesDto = mapper.Map<List<AddressDto>>(addresses);

            return StatusCode(addressesDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, addressesDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressById([Required] Guid id)
        {
            var categories = await addressService.GetByIdAsync(id);

            var categoryDto = mapper.Map<AddressDto>(categories);

            return StatusCode(categoryDto != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, categoryDto);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([Required] AddressRequest request)
        {
            var addressDb = await addressService.GetByIdAsync(request.Id);

            if (addressDb != null)
            {
                var address = mapper.Map<Address>(request);

                if (address != null)
                {
                    await addressService.UpdateAsync(address);

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
            var isDeleted = await addressService.DeleteAsync(id);

            return StatusCode(isDeleted ? StatusCodes.Status200OK : StatusCodes.Status404NotFound);
        }
    }
}
