using Test.Backend.Abstractions.Models.Dto.Common;
using Test.Backend.Abstractions.Models.Dto.Order;

namespace Test.Backend.Abstractions.Models.Dto.User
{
    public interface IOrders
    {
        public ICollection<OrderDto>? Orders { get; set; }
    }

    public class UserBaseDto : BaseDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }

    public class UserDto : UserBaseDto, IOrders
    {
        public ICollection<OrderDto>? Orders { get; set; }
    }
}
