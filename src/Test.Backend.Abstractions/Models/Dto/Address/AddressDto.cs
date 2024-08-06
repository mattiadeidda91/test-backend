using Test.Backend.Abstractions.Models.Dto.Common;
using Test.Backend.Abstractions.Models.Dto.Order;

namespace Test.Backend.Abstractions.Models.Dto.Address
{
    public interface IOrders
    {
        public ICollection<OrderWithoutAddressDto>? Orders { get; set; }
    }

    public class AddressBaseDto : BaseDto
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }

    public class AddressDto : AddressBaseDto, IOrders
    {
        public ICollection<OrderWithoutAddressDto>? Orders { get; set; }
    }
}
